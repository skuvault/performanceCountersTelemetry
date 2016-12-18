module PerformanceCounterHelper
open System.Collections.Concurrent
open System.Collections.Generic
open System.Diagnostics
open System.Threading
open System.Threading.Tasks
open System.Linq
open System
open Serilog
open Counters
open PerformanceCounterProxies

type PerformanceCounterHelper = 
    static member WriteLineCounter (counters: IDictionary< CounterAlias, CounterValue >) (writer: string -> unit) = 
        for counter in counters do 
            System.String.Format( "[{0}]\t[{1}]\t{2}", counter.Value.DateTime.ToString( "yyyy.MM.dd HH:mm:ss.fff"),counter.Key.Alias, counter.Value.Value) |> writer
    static member WriteLineCounterToConsole (counters: IDictionary< CounterAlias, CounterValue >) = 
        PerformanceCounterHelper.WriteLineCounter counters ( fun s -> System.Console.WriteLine s)
//    static member GetCountersOrNull ( instance:string)( counterCategory:PerformanceCounterCategory ) = 
//        try
//            if String.IsNullOrWhiteSpace( instance ) then counterCategory.GetCounters() else counterCategory.GetCounters( instance )
//        with
//        | _ as ex
//            -> null

//    static member GetCounter ( category: string) (counterName: string) (instance : string ) = 
//        let logWarnCounterNotFound (name:string) (instance:string) (category:string) = if instance = null then Log.Warning ( "Counter {name} not found in category {category}", counterName, category) else Log.Warning ("Counter {name}({instance}) not found in category {category}", counterName, instance, category)
//        let sideEffectOnNotNull act x = if x <> null then act(); x else x
//        let getCounterOrNull (name:string) (instance:string) (category:PerformanceCounterCategory) : PerformanceCounter = 
//            let instanceCounters = PerformanceCounterHelper.GetCountersOrNull instance category 
//            if instanceCounters = null then 
//                logWarnCounterNotFound counterName instance category.CategoryName; null
//            else
//                instanceCounters.FirstOrDefault( fun y -> String.Equals( y.CounterName, counterName, StringComparison.InvariantCultureIgnoreCase ) ) |> PerformanceCounterHelper.SideEffectOnNull ( fun unit -> logWarnCounterNotFound counterName instance category.CategoryName )
//        let getCategoryAndCounter category counterName instance =
//            Log.Information("Getting counter: {category}\\{name}\\{instance} ",  category, counterName, instance)
//            let counterCategory = PerformanceCounterCategory.GetCategories().FirstOrDefault( fun x -> String.Equals( x.CategoryName, category, StringComparison.InvariantCultureIgnoreCase ) )
//            if counterCategory = null  then
//                Serilog.Log.Warning ( "Category not found: {category}", category ); null
//            else
//                match counterCategory.CategoryType with
//                | PerformanceCounterCategoryType.MultiInstance -> getCounterOrNull counterName instance counterCategory
//                | PerformanceCounterCategoryType.SingleInstance -> getCounterOrNull counterName null counterCategory
//                | _ -> null
//
//        getCategoryAndCounter category counterName instance |> PerformanceCounterHelper.SideEffectOnNull (fun unit ->Log.Warning( "Counter not found: {category}\\{name}\\{instance}", category, counterName, instance )) 
//                                                            |> sideEffectOnNotNull (fun unit ->Log.Information( "Counter found: {category}\\{name}\\{instance}", category, counterName, instance ))
//    

