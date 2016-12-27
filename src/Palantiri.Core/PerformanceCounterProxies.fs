﻿module PerformanceCounterProxies
open Counters
open Serilog
open System.Diagnostics
open System
//open System.Collections.Concurrent
//open System.Collections.Generic
//open System.Collections
open System.Linq
open Utilities

type PerforrmanceCounterProxy( counter:Option<System.Diagnostics.PerformanceCounter>, fullname:CounterFullName, alias:CounterAlias )  = 
    let mutable _performanceCounter = counter
    member this.Alias = alias
    member this.Fullname = fullname
    member this.Counter with get() = lock this (fun () -> _performanceCounter)
                        and set(value) = lock this (fun () -> _performanceCounter <- value)

    new(counter,fullname) = PerforrmanceCounterProxy( counter, fullname, {Alias=null} )
    
    static member Create (counter, fullname, alias) = new PerforrmanceCounterProxy (counter,fullname,alias)

    static member GetPerformanceCountersFromCategoryOrNull ( instance:CounterInstance )( counterCategory:PerformanceCounterCategory ) = 
        try
            if  instance.Instance = null then counterCategory.GetCounters() else counterCategory.GetCounters( instance.Instance )
        with
        | _ as ex
            -> null

    static member GetPerformanceCounter ( fname: CounterFullName ) = 
        let getCounterInternal ( counterFullName: CounterFullName ) =
            Log.Information("Getting counter: {category}\\{name}\\{instance} ",  counterFullName.Category, counterFullName.Name, counterFullName.Instance)
            let receivedCategory = PerformanceCounterCategory.GetCategories().FirstOrDefault( fun x -> String.Equals( x.CategoryName, counterFullName.Category.Category, StringComparison.InvariantCultureIgnoreCase ) )
            if receivedCategory = null  then
                Serilog.Log.Warning ( "Category not found: {category}", counterFullName.Category ); null
            else
                let receivedCounters = PerforrmanceCounterProxy.GetPerformanceCountersFromCategoryOrNull counterFullName.Instance receivedCategory
                if receivedCounters = null then 
                    Log.Warning ("Instance not found {name}(instance: {instance}) in category {category}", counterFullName.Name, counterFullName.Instance, counterFullName.Category); null
                else
                    receivedCounters.FirstOrDefault( fun y -> String.Equals( y.CounterName, counterFullName.Name.Name, StringComparison.InvariantCultureIgnoreCase ) ) 
                    |> CommonHelper.SideEffectOnNull ( fun unit -> Log.Warning ("Name {name}(instance: {instance}) not found for category {category}", counterFullName.Name, counterFullName.Instance, counterFullName.Category) )
        getCounterInternal fname
            |> CommonHelper.SideEffectOnNull (fun unit ->Log.Warning( "Getting counter failed: {category}\\{name}\\{instance}", fname.Category, fname.Name, fname.Instance )) 
            |> CommonHelper.SideEffectOnNotNull (fun unit ->Log.Information( "Getting Counter secceed: {category}\\{name}\\{instance}", fname.Category, fname.Name, fname.Instance ))
            |> (fun x -> if x = null then Option.None else Option.Some(x))
    
    member this.ReFresh () = 
        try
            this.Counter <- PerforrmanceCounterProxy.GetPerformanceCounter this.Fullname 
        with
        | _ as ex
            -> Serilog.Log.Error( ex, "Performance Counter {category}//{name}//{instance} can't be refreshed", this.Fullname.Category,this.Fullname.Name,this.Fullname.Instance )
