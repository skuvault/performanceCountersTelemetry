module PerformanceCounterProxies

type PerforrmanceCounterProxy( counter:System.Diagnostics.PerformanceCounter, alias:string ) = 
    let mutable _performanceCounter = counter
    member this.Alias = alias
    new(counter:System.Diagnostics.PerformanceCounter) = PerforrmanceCounterProxy(counter, null)
    
    member this.Counter
        with get() = lock this (fun () -> _performanceCounter)
    member this.ReFresh = () (* lock this ( fun () -> *)
                                       (* _performanceCounter = PerformanceCounterHelper *)
