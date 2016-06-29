Palantiri
=============
A C# client to performance counters.

##Usage

##Start
App should be started with parameters to catch system counters. Multiple instances cab be started in one machine. Each instance scans specified counters and sends them to specified destinations. 

Arguments example:

``` 
CreateSensorAndStart -p param.json
```

Parameters example:
``` 
{
	"Counters": [
		{
			"Category": "Processor Information",
			"Name": "% Processor Time",
			"Instance": "_Total",
			"Alias": "cpu-total"
		}
	],

	"Destinations": [
		{
			"Name": "Console"
		}
	],

	"Period": 1000
}
```

After start app receives all counters (specified in parameters) and send them to destinations (specified in parameters).

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
