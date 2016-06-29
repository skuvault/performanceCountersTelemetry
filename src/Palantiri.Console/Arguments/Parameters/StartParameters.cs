using PowerArgs;

namespace Palantiri.Console.Arguments.Parameters
{
	public class StartParameters
	{
		//[ ArgRequired, ArgDescription( "Counters" ), ArgShortcut( "cs" ), ArgExample( ".NET CLR Memory;_Global_;% Time in GC;devenv.exe;SomeAlias|.NET CLR Memory;_Global_;Allocated Bytes/sec;devenv.exe", "Counters full name and aliases" ) ]
		//public string CountersList{ get; set; }

		//[ ArgRequired, ArgDescription( "Destinations" ), ArgShortcut( "ds" ), ArgExample( "Console|Telegraf", "Counters destinations" ) ]
		//public string DestinationsList{ get; set; }

		//[ ArgDefaultValue( ";" ), ArgDescription( "Counters/Destination Separator" ), ArgShortcut( "gs" ), ArgExample( ";", "Counters/Destination Separator" ) ]
		//public string GlobalSeparator{ get; set; }

		[ArgDescription("JSON file with parameters"), ArgShortcut("p"), ArgExample("someCounters.json", "some counters")]
		public string Path { get; set; }
	}
}