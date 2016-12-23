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
    abstract member AddObservers : seq<ISensorObserver>-> unit
    abstract member RemoveObservers: seq<ISensorObserver> -> seq<bool>
    abstract member NotifyObservers: ConcurrentDictionary< CounterAlias, CounterValue > -> unit

type Sensor( periosMs:int, recreationPeriodMs:int, counters:seq<PerforrmanceCounterProxy>) =
    let mutable _counters = counters
    let mutable _periodMs = periosMs
    let mutable _recreationPeriodMs = recreationPeriodMs
    let mutable _countersQueue = new ConcurrentQueue< ConcurrentDictionary< string, float > >()
    let _observers = new List< ISensorObserver >()

    let mutable _startLock = new System.Object()
    let mutable _sensorCts = new CancellationTokenSource()
    let mutable _sensorCt = new CancellationToken()
    let mutable _sensorTask = new Task(null)

    interface ISensorObservable with 
        member this.AddObservers observers = observers |> Seq.iter _observers.Add
        member this.RemoveObservers observers = observers |> Seq.map _observers.Remove
        member this.NotifyObservers counters = _observers |> Seq.iter (fun o -> o.SendCounters counters)
    
    static member GetCounterAlias (pc:PerformanceCounter) = pc.CategoryName + "_" + pc.CounterName + "_" + pc.InstanceName
    
    member this.GetCountersValues () = 
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
        lock _startLock ( fun ()->  _sensorCts.Cancel() )
        Log.Information( "Sensor stopped." )

    member this.Start() = 
        let readSensorAndNotify () = this.GetCountersValues() |> (this :> ISensorObservable).NotifyObservers; Log.Debug("Sensor observers notified")
        let readSensorAndNotifyInfinite () = while not _sensorCt.IsCancellationRequested do 
                                                readSensorAndNotify() 
                                                Task.Delay( _periodMs ).Wait()
        let startSensor () = if _sensorCts.IsCancellationRequested then
                                _sensorCts <- new CancellationTokenSource()
                                _sensorCt <- _sensorCts.Token
                                _sensorTask <-  Task.Factory.StartNew readSensorAndNotifyInfinite
                             _sensorCts.IsCancellationRequested
        Log.Information( "Starting sensor..." ) 
        lock _startLock (fun ()-> if startSensor() then Log.Information( "Sensor started." ) else Log.Information( "Sensor started. (Had already started, init skiped)" ))

        
    static member GetCounters (onNotFound : Option<CounterFullName*CounterAlias->unit> ) ( counters: seq<CounterFullName*CounterAlias>)  = 
        Log.Debug ( "Start getting counters..." )
        let getCounterOrNull (cFullName,cAlias) = 
            let pc  = PerforrmanceCounterProxy.GetPerformanceCounter cFullName
            if pc = null && (onNotFound.IsSome) then onNotFound.Value (cFullName,cAlias)
            (pc,cFullName,cAlias)

//        let getCounterProxy (perfCounter,cntr:string[]) =   let alias = if cntr.Length > 3 && not (String .IsNullOrWhiteSpace cntr.[3]) then cntr.[3] else Sensor.GetCounterAlias perfCounter 
//                                                            new PerforrmanceCounterProxy (perfCounter, {Alias = alias},{})

        let result = counters |> Seq.map getCounterOrNull |> Seq.filter (fun (pc,fn,a) -> pc <> null) |> Seq.map PerforrmanceCounterProxy.Create
        Log.Debug( "End getting counters." )
        result
        