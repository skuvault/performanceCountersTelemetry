module Counters
open System

type CounterAlias( alias:string ) = 
    member this.Alias = if alias = null then String.Empty else alias
    static member Create alias = new CounterAlias (alias)
    static member Empty = new CounterAlias (String.Empty)

type CounterCategory( category:string ) = 
    member this.Category =  if category = null then String.Empty else category
    static member Create category = new CounterCategory( category )

type CounterInstance( instance:string ) = 
    member this.Instance = if instance = null then String.Empty else instance
    member this.IsEmpty() = this.Instance = String.Empty
    static member Create instance = new CounterInstance( instance )
    static member Empty = new CounterInstance (String.Empty)

type CounterName( name:string ) = 
    member this.Name =  if name = null then String.Empty else name
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
