module SensorManager
open SensorObservable
open System.Collections.Generic
open System.Collections
open System.Linq
open Serilog

type SensorManager private () =
    static let instance = SensorManager()
    static let Sensors = List< Sensor >()
    static member GetSingleton = instance
    member this.AddSensors sensors = Sensors.AddRange<|sensors
    member this.GetSensorsCount sensors = Sensors.Count
    member this.GetSensorTask sensors = 
        try
            Some<|Sensors.First()
        with
            | _ as ex -> Log.Error(ex, "There is no sensor"); None
