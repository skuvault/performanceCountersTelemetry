module PerformanceCounterProxies
open Counters
open Serilog
open System.Diagnostics
open System
//open System.Collections.Concurrent
//open System.Collections.Generic
//open System.Collections
open System.Linq
open Utilities

type PerforrmanceCounterProxy( counter:System.Diagnostics.PerformanceCounter, alias:CounterAlias ) = 
    let mutable _performanceCounter = counter
    member this.Alias = alias
    member this.Counter with get() = lock this (fun () -> _performanceCounter)
    new(counter) = PerforrmanceCounterProxy( counter, CounterAlias.Empty )

    static member GetCountersOrNull ( instance:string )( counterCategory:PerformanceCounterCategory ) = 
        try
            if String.IsNullOrWhiteSpace( instance ) then counterCategory.GetCounters() else counterCategory.GetCounters( instance )
        with
        | _ as ex
            -> null

    static member GetCounter ( category: string) (counterName: string) (instance : string ) = 
        let logWarnCounterNotFound (name:string) (instance:string) (category:string) = if instance = null then Log.Warning ( "Counter {name} not found in category {category}", counterName, category) else Log.Warning ("Counter {name}({instance}) not found in category {category}", counterName, instance, category)
        let sideEffectOnNotNull act x = if x <> null then act(); x else x
        let getCounterOrNull (name:string) (instance:string) (category:PerformanceCounterCategory) : PerformanceCounter = 
            let instanceCounters = PerforrmanceCounterProxy.GetCountersOrNull instance category 
            if instanceCounters = null then 
                logWarnCounterNotFound counterName instance category.CategoryName; null
            else
                instanceCounters.FirstOrDefault( fun y -> String.Equals( y.CounterName, counterName, StringComparison.InvariantCultureIgnoreCase ) ) |> CommonHelper.SideEffectOnNull ( fun unit -> logWarnCounterNotFound counterName instance category.CategoryName )
        let getCategoryAndCounter category counterName instance =
            Log.Information("Getting counter: {category}\\{name}\\{instance} ",  category, counterName, instance)
            let counterCategory = PerformanceCounterCategory.GetCategories().FirstOrDefault( fun x -> String.Equals( x.CategoryName, category, StringComparison.InvariantCultureIgnoreCase ) )
            if counterCategory = null  then
                Serilog.Log.Warning ( "Category not found: {category}", category ); null
            else
                match counterCategory.CategoryType with
                | PerformanceCounterCategoryType.MultiInstance -> getCounterOrNull counterName instance counterCategory
                | PerformanceCounterCategoryType.SingleInstance -> getCounterOrNull counterName null counterCategory
                | _ -> null

        getCategoryAndCounter category counterName instance |> CommonHelper.SideEffectOnNull (fun unit ->Log.Warning( "Counter not found: {category}\\{name}\\{instance}", category, counterName, instance )) 
                                                            |> sideEffectOnNotNull (fun unit ->Log.Information( "Counter found: {category}\\{name}\\{instance}", category, counterName, instance ))
    
    
    member this.ReFresh () = 
        try
            lock this ( fun () -> _performanceCounter <- PerforrmanceCounterProxy.GetCounter _performanceCounter.CategoryName _performanceCounter.CounterName _performanceCounter.InstanceName )
        with
        | _ as ex
            -> Serilog.Log.Error( ex, "Performance Counter {categoryName}//{counterName}//{instanceName} can't be refreshed", _performanceCounter.CategoryName, _performanceCounter.CounterName, _performanceCounter.InstanceName )
