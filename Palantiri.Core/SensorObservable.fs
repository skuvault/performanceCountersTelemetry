module SensorObservable
open System.Collections.Concurrent
open System.Collections.Generic
open System.Collections
open System.Diagnostics
open System.Threading
open System.Threading.Tasks
open Counters
open PerformanceCounterProxies
open SensorObserver

type ISensorObservable =
    abstract member AddObservers : ISensorObserver[]-> unit
    abstract member RemoveObserver: ISensorObserver -> unit
    abstract member NotifyObservers: ConcurrentDictionary< CounterAlias, CounterValue > -> unit

type Sensor( periosMs:int, recreationPeriodMs:int, counters:PerforrmanceCounterProxy[]) =
    let mutable _counters = counters
    let mutable _periodMs = periosMs
    let mutable _recreationPeriodMs = recreationPeriodMs
    let mutable _countersQueue = new ConcurrentQueue< ConcurrentDictionary< string, float > >()
    let mutable _observers = new List< ISensorObserver >()

    interface ISensorObservable with 
        member this.AddObservers observers = observers |> Seq.iter _observers.Add
        member this.RemoveObserver observer = _observers.Remove observer |> ignore
        member this.NotifyObservers counters = _observers |> Seq.iter (fun o -> o.SendCounters counters)
    
    static member GetCounterId (pc:PerformanceCounter) = pc.CategoryName + "_" + pc.CounterName + "_" + pc.InstanceName