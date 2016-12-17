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
    
    static member GetCounterId (pc:PerformanceCounter) = pc.CategoryName + "_" + pc.CounterName + "_" + pc.InstanceName
    
    member this.GetCounterValues () = 
        Log.Debug ( "Getting counters values..." )
        let dateTime = System.DateTime.UtcNow
        let getCounterValueAndPutToAcc (state:ConcurrentDictionary< CounterAlias, CounterValue >) (x:PerforrmanceCounterProxy)  = 
            try
                let nextValue = float <| x.Counter.NextValue()
                state.AddOrUpdate( new CounterAlias( x.Alias ), new CounterValue( dateTime, nextValue ), ( fun cid y -> new CounterValue( dateTime, nextValue ))) |> ignore
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