module SensorObservable
open System.Collections.Concurrent
open System.Collections.Generic
open System.Collections
open System.Threading
open System.Threading.Tasks
open Counters
open PerformanceCounterProxies
//open SensorObserver

//type ISensorObservable =
//    abstract member AddObservers: ISensorObserver[] -> unit
//    abstract member RemoveObserver: ISensorObserver -> unit
//    abstract member NotifyObservers: ConcurrentDictionary< CounterAlias, CounterValue > -> unit