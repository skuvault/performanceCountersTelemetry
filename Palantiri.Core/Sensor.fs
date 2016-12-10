module Sensor
open System.Collections.Concurrent
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks
open Counters

type ISensorObserver =
    abstract member SendCounters: ConcurrentDictionary< CounterAlias, CounterValue > -> unit

type PcHelper() = 
    static member WriteLineCounter (counters: IDictionary< CounterAlias, CounterValue >) (writer: string -> unit) = 
        for counter in counters do 
            System.String.Format( "[{0}]\t[{1}]\t{2}", counter.Value.DateTime.ToString( "yyyy.MM.dd HH:mm:ss.fff"), counter.Key.Alias, counter.Value.Value) |> writer
    static member WriteLineCounterToConsole (counters: IDictionary< CounterAlias, CounterValue >) = 
        PcHelper.WriteLineCounter counters ( fun s -> System.Console.WriteLine s)

type ConsoleObserver() = 
    let _period = 500
    let _cts = new CancellationTokenSource()
    let _ct = _cts.Token
    let _maxInstancesToProcess = 10
    let mutable _buffer = new ConcurrentQueue< ConcurrentDictionary< CounterAlias, CounterValue > >()
    let _consoleWriter = Task.Factory.StartNew( fun () -> 
            while not _ct.IsCancellationRequested do
                if _buffer <> null then
                    let mutable temp = new ConcurrentDictionary< CounterAlias, CounterValue >()
                    seq { for x in 1 .. _maxInstancesToProcess -> if _buffer.TryDequeue( &temp ) then Some( temp ) else None }
                    |> Seq.takeWhile ( fun x -> x <> Option.None )
                    |> Seq.iter ( fun x -> PcHelper.WriteLineCounterToConsole(x.Value))
                Task.Delay(100).Wait()
                )

    interface ISensorObserver with 
        member this.SendCounters counters = 
            _buffer.Enqueue( counters )

    interface System.IDisposable with 
        member this.Dispose() = 
         if _consoleWriter <> null && _cts <> null && _cts.IsCancellationRequested then
            _cts.Cancel()

type ObserverFactory() =
    static member CreateObserver (observer:string) (par:string) = 
        let consoleObserver = null
        match observer.ToLowerInvariant() with
        | "console" -> null
        | _ -> null