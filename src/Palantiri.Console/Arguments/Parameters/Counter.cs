using PowerArgs;

namespace Palantiri.Console.Arguments.Parameters
{
	public class Counter
	{
		[ ArgRequired, ArgDescription( "Category" ), ArgShortcut( "c" ), ArgExample( ".NET CLR Memory", "Category name" ), ArgPosition( 1 ) ]
		public string Category{ get; set; }

		[ ArgRequired, ArgDescription( "Name" ), ArgShortcut( "n" ), ArgPosition( 2 ) ]
		public string Name{ get; set; }

		[ ArgRequired, ArgDescription( "Instance" ), ArgShortcut( "i" ), ArgPosition( 3 ) ]
		public string Instance{ get; set; }

		[ ArgDescription( "Alias" ), ArgShortcut( "a" ), ArgPosition( 3 ) ]
		public string Alias{ get; set; }
	}
}