module Counters

type CounterAlias( alias:string ) = 
    member this.Alias = alias
    static member Create alias = new CounterAlias (alias)

type CounterCategory( category:string ) = 
    member this.Category = category
    static member Create category = new CounterCategory( category )

type CounterInstance( instance:string ) = 
    member this.Instance = instance
    static member Create instance = new CounterInstance( instance )

type CounterName( name:string ) = 
    member this.Name = name
    static member Create name = new CounterName( name )

type CounterFullName( name:CounterName, category:CounterCategory, instance:CounterInstance ) = 
    member this.Name = name
    member this.Category = category
    member this.Instance = instance
    static member Create name category instance = new CounterFullName( name, category, instance )

type CounterValue( dateTime:System.DateTime, value:float ) = 
    member this.Value = dateTime
    member this.DateTime = dateTime
    static member Create dateTime value = new CounterValue( dateTime, value )
