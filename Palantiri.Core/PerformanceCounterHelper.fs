module PerformanceCounterHelper
open System.Collections.Concurrent
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks
open Counters

type PerformanceCounterHelper = 
    static member WriteLineCounter (counters: IDictionary< CounterAlias, CounterValue >) (writer: string -> unit) = 
        for counter in counters do 
            System.String.Format( "[{0}]\t[{1}]\t{2}", counter.Value.DateTime.ToString( "yyyy.MM.dd HH:mm:ss.fff"), counter.Key.Alias, counter.Value.Value) |> writer
    static member WriteLineCounterToConsole (counters: IDictionary< CounterAlias, CounterValue >) = 
        PerformanceCounterHelper.WriteLineCounter counters ( fun s -> System.Console.WriteLine s)
