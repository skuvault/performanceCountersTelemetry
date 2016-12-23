﻿module CommandsSets
open SensorObservable
open PowerArgs
open Serilog
open System.IO
open Newtonsoft.Json
open System.Text
open Counters
open SensorObserver

type Destination = {
    [<ArgRequired;  ArgDescription( "Name" );       ArgShortcut( "dn" );    ArgExample( "Telegraf", "Destination Name" )                    >]Name:string;
    [<              ArgDescription( "Parameters" ); ArgShortcut( "dc" );    ArgExample( "500 1000 environemnt sysName id", "Parameters" )   >]Parameters:string}
type Counter = {
    [<ArgRequired;  ArgDescription( "Category" );   ArgShortcut( "c" ); ArgExample( ".NET CLR Memory", "Category name" ) >]Category:string;
    [<ArgRequired;  ArgDescription( "Name" );       ArgShortcut( "n" )                                                   >]Name:string;
    [<ArgRequired;  ArgDescription( "Instance" );   ArgShortcut( "i" )                                                   >]Instance:string;
    [<              ArgDescription( "Alias" );      ArgShortcut( "a" )                                                   >]Alias:string}
type JsonConfig = {
    Counters:seq<Counter>;
    Destinations:seq<Destination>;
    Period:int;
    RecreationPeriodMs:int}

type CreateSensorAndStartParameters () = 
    [<ArgDescription( "JSON file with parameters" ); ArgShortcut( "p" ); ArgExample( "counters.json", "counters" )>] member val Path = ""
    member this.CreateSensor () = 
        Log.Debug( "Start json parametrs ( " + this.Path + " ) reading..." )
        use streamReader = new StreamReader( this.Path, Encoding.UTF8 )
        let jsonConfig = JsonConvert.DeserializeObject< JsonConfig >( streamReader.ReadToEnd() )
        Log.Debug( "End json parametrs ( " + this.Path + " ) reading." )
        
        Log.Debug( "Start counters parsing..." )
        let cntrs = jsonConfig.Counters 
                        |> Seq.map (fun x -> ({ CounterFullName.Name = {Name = x.Name}; Category = {Category = x.Category}; Instance = {Instance = x.Instance} }, {CounterAlias.Alias = x.Alias}))
                        |> Sensor.GetCounters Option.None
        Log.Debug( "End counters parsing." )

        Log.Debug( "Start observers parsing..." )
        let obsrvrs = jsonConfig.Destinations 
                        |> Seq.map (fun x -> if x.Parameters = null then { x with Parameters = ""} else x )
                        |> Seq.map (fun x -> ObserverFactory.CreateObserver x.Name <| Args.Convert x.Parameters )
        Log.Debug( "End observers parsing." )

        let sensor = new Sensor( jsonConfig.Period, jsonConfig.RecreationPeriodMs, cntrs ) 
        (sensor :> ISensorObservable).AddObservers obsrvrs
        sensor


[<ArgExceptionBehavior( ArgExceptionPolicy.StandardExceptionHandling )>]
type Commands() =     
    [<HelpHook; ArgShortcut( "-?" ); ArgDescription( "Shows this help" )>] member val Help = false
    [<ArgShortcut( "-?" ); ArgDescription( "Shows this help" )>] member this.CreateSensorAndStart (args:CreateSensorAndStartParameters) = (*let s = args.CreateSensor();*) ()
        

