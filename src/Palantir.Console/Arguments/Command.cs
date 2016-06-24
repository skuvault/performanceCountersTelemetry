using System;
using System.Linq;
using Palantir;
using Palantir.SensorObservers;
using Palantir.Sensors;
using PowerArgs;

namespace PerfCoun.Console.Arguments
{
	[ ArgExceptionBehavior( ArgExceptionPolicy.StandardExceptionHandling ) ]
	public class ConsoleCommands
	{
		[ HelpHook, ArgShortcut( "-?" ), ArgDescription( "Shows this help" ) ]
		public bool Help{ get; set; }

		[ ArgActionMethod, ArgDescription( "Adds the two operands" ) ]
		public void AddCounter( Counter args )
		{
			//Console.WriteLine(args.Category + args.Instance);
		}

		[ ArgActionMethod, ArgDescription( "Start Sensor" ) ]
		public void Start( Counters args )
		{
			var countersParsed = args.CountersList.Split( '|' ).Select( countersFullName => countersFullName.Split( ';' ) ).ToList();
			var observersParsed = args.DestinationsList.Split( '|' );

			Action< string, string, string > notifyNotFound = ( x, y, z ) =>
			{
				System.Console.WriteLine( "Not found: {0},{1},{2}", x, y, z );
			};
			
			var sensor = new Sensor( 1000, PerformanceCounterHelper.GetCounters( countersParsed, notifyNotFound ).ToArray() );
			sensor.AddObservers( observersParsed.Select( x => x.CreateObserver() ).Where( y => y != null ).ToArray() );
			sensor.Start();
			Program.AddSensorTasks( sensor );
		}
	}

	public class Counter
	{
		[ ArgRequired, ArgDescription( "Category" ), ArgExample( ".NET CLR Memory", "Category name" ), ArgPosition( 1 ) ]
		public double Category{ get; set; }

		[ ArgRequired, ArgDescription( "Name" ), ArgPosition( 2 ) ]
		public double Name{ get; set; }

		[ ArgRequired, ArgDescription( "Instance" ), ArgPosition( 3 ) ]
		public double Instance{ get; set; }
	}

	public class Counters
	{
		[ ArgRequired, ArgDescription( "Counters" ), ArgShortcut( "cs" ), ArgExample( ".NET CLR Memory;_Global_;% Time in GC;devenv.exe;SomeAlias|.NET CLR Memory;_Global_;Allocated Bytes/sec;devenv.exe", "Counters full name and aliases" ), ArgPosition( 1 ) ]
		public string CountersList{ get; set; }

		[ ArgRequired, ArgDescription( "Destinations" ), ArgShortcut( "ds" ), ArgExample( "Console|Telegraf", "Counters destinations" ), ArgPosition( 2 ) ]
		public string DestinationsList{ get; set; }
	}
}