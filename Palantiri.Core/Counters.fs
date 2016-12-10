module Counters

type CounterAlias( alias:string ) = 
    member this.Alias = alias

type CounterCategory( category:string ) = 
    member this.Category = category

type CounterInstance( instance:string ) = 
    member this.Instance = instance

type CounterName( name:string ) = 
    member this.Name = name

type CounterFullName( name:CounterName, category:CounterCategory, instance:CounterInstance ) = 
    member this.Name = name
    member this.Category = category
    member this.Instance = instance

type CounterValue( dateTime:System.DateTime, value:float ) = 
    member this.Value = dateTime
    member this.DateTime = dateTime
