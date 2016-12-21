module Counters
open System

type CounterAlias = {Alias:string}
type CounterCategory = {Category:string}
type CounterInstance = {Instance:string}
type CounterName = {Name:string}
type CounterFullName = {Name:CounterName; Category:CounterCategory; Instance:CounterInstance}
type CounterValue = {ActualOn:System.DateTime; Value:float}
//
//type CounterCategory( category:string ) = 
//    member this.Category =  if category = null then String.Empty else category
//    static member Create category = new CounterCategory( category )
//
//type CounterInstance( instance:string ) = 
//    member this.Instance = if instance = null then String.Empty else instance
//    member this.IsEmpty() = this.Instance = String.Empty
//    static member Create instance = new CounterInstance( instance )
//    static member Empty = new CounterInstance (String.Empty)
//
//type CounterName( name:string ) = 
//    member this.Name =  if name = null then String.Empty else name
//    static member Create name = new CounterName( name )
//
//type CounterFullName( name:CounterName, category:CounterCategory, instance:CounterInstance ) = 
//    member this.Name = name
//    member this.Category = category
//    member this.Instance = instance
//    static member Create name category instance = new CounterFullName( name, category, instance )
//
//type CounterValue( dateTime:System.DateTime, value:float ) = 
//    member this.Value = dateTime
//    member this.DateTime = dateTime
//    static member Create dateTime value = new CounterValue( dateTime, value )
