using PowerArgs;

namespace Palantiri.Console.Arguments.Parameters
{
	public class StartParameters
	{
		[ ArgDescription( "JSON file with parameters" ), ArgShortcut( "p" ), ArgExample( "someCounters.json", "some counters" ) ]
		public string Path{ get; set; }
	}
}