module SensorObservable
open System.Collections.Concurrent
open System.Collections.Generic
open System.Collections
open System.Diagnostics
open System.Threading
open System.Threading.Tasks
open System
open Counters
open PerformanceCounterProxies
open PerformanceCounterHelper
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

    let mutable _startLock = new System.Object()
    let mutable _started = false
    let mutable _sensorCts = new CancellationTokenSource()
    let mutable _sensorCt = new CancellationToken()
    let mutable _sensorTask = new Task(null)

    interface ISensorObservable with 
        member this.AddObservers observers = observers |> Seq.iter _observers.Add
        member this.RemoveObserver observer = _observers.Remove observer |> ignore
        member this.NotifyObservers counters = _observers |> Seq.iter (fun o -> o.SendCounters counters)
    
    static member GetCounterAlias (pc:PerformanceCounter) = pc.CategoryName + "_" + pc.CounterName + "_" + pc.InstanceName
    
    member this.GetCounterValues () = 
        Log.Debug ( "Getting counters values..." )
        let dateTime = System.DateTime.UtcNow
        let getCounterValueAndPutToAcc (state:ConcurrentDictionary< CounterAlias, CounterValue >) (x:PerforrmanceCounterProxy)  = 
            try
                let nextValue = float <| x.Counter.NextValue()
                state.AddOrUpdate( x.Alias, { ActualOn=dateTime; Value=nextValue }, ( fun cid y -> { ActualOn=dateTime; Value=nextValue })) |> ignore
                Log.Information( "Counter value received: [{alias}][{timepoint}][{value}].", x.Alias, dateTime, nextValue )
                state
            with
            | _ as ex -> Log.Error(ex, "Can't get counter:  "+x.ToString()); state

        let counters = _counters |> Seq.fold getCounterValueAndPutToAcc (new ConcurrentDictionary< CounterAlias, CounterValue >())
        Log.Information( "Counters values received." )
        counters

    member this.Stop() = 
        Log.Information( "Stopping sensor..." )
        lock _startLock ( fun ()->  _sensorCts.Cancel(); _started <- false )
        Log.Information( "Sensor stopped." )

    member this.Start() = 
        let readSensorAndNotify () = this.GetCounterValues() |> (this :> ISensorObservable).NotifyObservers; Log.Debug("Sensor observers notified")
        let readSensorAndNotifyInfinite () = while _started && not _sensorCt.IsCancellationRequested do 
                                                readSensorAndNotify() 
                                                Task.Delay( _periodMs ).Wait()
        let startSensor () = if not _started then
                                _started <- true
                                _sensorCts <- new CancellationTokenSource()
                                _sensorCt <- _sensorCts.Token
                                _sensorTask <-  Task.Factory.StartNew readSensorAndNotifyInfinite
        Log.Information( "Starting sensor..." ) 
        lock _startLock startSensor
        Log.Information( "Sensor started." )

//    member this.RemoveCounters( counters:seq<PerforrmanceCounterProxy> ) (onRemoved: CounterAlias -> unit) = 
//        let getCounterByAlias ( cntrs:seq<PerforrmanceCounterProxy> ) (alias: CounterAlias ) = cntrs |> Seq.tryFind (fun cntr -> cntr.Alias = alias)
//        let removeCounters () = _counters <- _counters 
//                                            |> Seq.filter (fun _c ->    let shouldBeRemoved = ( getCounterByAlias counters _c.Alias ) <> None
//                                                                        if shouldBeRemoved then Log.Information( "Counter marked for remove: {@counter}.", _c ); onRemoved _c.Alias
//                                                                        not shouldBeRemoved)
//                                            |> Seq.toArray
//        Log.Information( "Removing counters..." )
//        lock _startLock removeCounters
//        Log.Information( "Counters removed {@counters}.", counters )

    member this.AddCounters( counters:seq<PerforrmanceCounterProxy> ) = 
        let addCounters () = _counters <- _counters |> Seq.ofArray |> Seq.append counters |> Seq.toArray
        Log.Information( "Adding counters..." )
        lock _startLock addCounters
        Log.Information( "Counters added: {@counters}.", counters )

    static member GetCounters ( counters: seq<CounterFullName*CounterAlias>) (onNotFound : Option<CounterFullName*CounterAlias->unit> ) = 
        Log.Debug ( "Start getting counters..." )
        let getCounterOrNull (cFullName,cAlias) = 
            let pc  = PerforrmanceCounterProxy.GetCounter cFullName
            if pc = null && (onNotFound.IsSome) then onNotFound.Value (cFullName,cAlias)
            (pc,cFullName,cAlias)

//        let getCounterProxy (perfCounter,cntr:string[]) =   let alias = if cntr.Length > 3 && not (String .IsNullOrWhiteSpace cntr.[3]) then cntr.[3] else Sensor.GetCounterAlias perfCounter 
//                                                            new PerforrmanceCounterProxy (perfCounter, {Alias = alias},{})

        let result = counters |> Seq.map getCounterOrNull |> Seq.filter (fun (pc,fn,a) -> pc <> null) |> Seq.map PerforrmanceCounterProxy.Create
        Log.Debug( "Counters received" )
        result