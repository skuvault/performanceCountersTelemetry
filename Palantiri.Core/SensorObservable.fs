﻿module SensorObservable
open System.Collections.Concurrent
open System.Collections.Generic
open System.Collections
open System.Diagnostics
open System.Threading
open System.Threading.Tasks
open Counters
open PerformanceCounterProxies
open SensorObserver
open Serilog

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
    
    member this.GetCounterValues = 
        Log.Debug ( "Getting counters values..." )
        let dateTime = System.DateTime.UtcNow

        let counters = _counters |> Seq.fold (fun (state:ConcurrentDictionary< CounterAlias, CounterValue >) x ->
                                                let nextValue = float <| x.Counter.NextValue()
                                                state.AddOrUpdate(new CounterAlias( x.Alias ) , new CounterValue( dateTime, nextValue ), ( fun cid y -> new CounterValue( dateTime, nextValue )))
                                                Log.Information( "Counter value received: [{alias}][{timepoint}][{value}].", x.Alias, dateTime, nextValue )
                                                state
                                            ) 
                                    (new ConcurrentDictionary< CounterAlias, CounterValue >())
        Log.Information( "Counters values received." )
        counters

        