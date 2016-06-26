using PowerArgs;

namespace Palantiri.Console.Arguments.Parameters
{
	public class Counters
	{
		[ ArgRequired, ArgDescription( "Counters" ), ArgShortcut( "cs" ), ArgExample( ".NET CLR Memory;_Global_;% Time in GC;devenv.exe;SomeAlias|.NET CLR Memory;_Global_;Allocated Bytes/sec;devenv.exe", "Counters full name and aliases" ), ArgPosition( 1 ) ]
		public string CountersList{ get; set; }

		[ ArgRequired, ArgDescription( "Destinations" ), ArgShortcut( "ds" ), ArgExample( "Console|Telegraf", "Counters destinations" ), ArgPosition( 2 ) ]
		public string DestinationsList{ get; set; }

		[ ArgRequired, ArgDescription( "Counters/Destination Separator" ), ArgShortcut( "gs" ), ArgExample( ";", "Counters/Destination Separator" ), ArgPosition( 3 ) ]
		public string GlobalSeparator{ get; set; }

		//[ ArgRequired, ArgDescription( "Counters/Destination parameters Separator" ), ArgShortcut( "ps" ), ArgExample( ",", "Counters parameters separator" ), ArgPosition( 4 ) ]
		//public string ParametersSeparator{ get; set; }
	}
}