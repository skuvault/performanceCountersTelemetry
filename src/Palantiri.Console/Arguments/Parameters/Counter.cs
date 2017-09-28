using PowerArgs;

namespace Palantiri.Console.Arguments.Parameters
{
	public class Counter
	{
		[ ArgRequired, ArgDescription( "Category" ), ArgShortcut( "c" ), ArgExample( ".NET CLR Memory", "Category name" ) ]
		public string Category{ get; set; }

		[ ArgRequired, ArgDescription( "Name" ), ArgShortcut( "n" ) ]
		public string Name{ get; set; }

		[ ArgRequired, ArgDescription( "Instance" ), ArgShortcut( "i" ) ]
		public string Instance{ get; set; }

		[ ArgDescription( "Alias" ), ArgShortcut( "a" ) ]
		public string Alias{ get; set; }

		[ArgDescription("DevideByCpuCoresCount"), ArgShortcut("dbccc")]
		public bool DevideByCpuCoresCount { get; set; }
	}
}