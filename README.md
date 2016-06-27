Palantiri
=============
A C# client to performance counters.

##Usage

##Start
App should be started with parameters to catch system counters. Multiple instances cab be started in one machine. Each instance scans specified counters and sends them to specified destinations. 

Arguments example:

``` 
Start -cs "-c \".NET CLR Memory\" -i \"w3wp\" -n \"%% Time in GC\" -a \"w3wp-in-gc\"; -c \".NET CLR Memory\" -i \"_Global_\" -n \"%% Time in GC\" -a \"global-in-gc\"; -c \"Processor Information\" -n \"_Total\" -i \"%% Processor Time\" -a \"total-cpu-time\"" -ds "File" -ll "Debug" -ld "ColoredConsole"
```

After start app receives all the specified counters and send them to destinations.

##Add Counters

Counters can be added on the fly. Example:
```
AddCounter -c "Processor information" -i "%% Processor usage" -n "_Total" -a "cpu"
```

##Help 
``` 
-?
```


##Development
* Please have a chat about any big features before submitting PR's
* New destinations and additional control will be added if it need
