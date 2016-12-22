module CommandsSets
open PowerArgs

[<ArgExceptionBehavior( ArgExceptionPolicy.StandardExceptionHandling )>]
type Commands() =     
    [<HelpHook; ArgShortcut( "-?" ); ArgDescription( "Shows this help" )>] member val Help = false

