// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open Serilog
open PowerArgs
open System.Reflection

[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    Log.Information( "Start Palantiri ver:" + Assembly.GetExecutingAssembly().GetName().Version.ToString() )
    0 // return an integer exit code
