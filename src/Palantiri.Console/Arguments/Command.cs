using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Palantiri.SensorObservers;
using Palantiri.Sensors;
using PowerArgs;

namespace Palantiri.Console.Arguments
{
	[ ArgExceptionBehavior( ArgExceptionPolicy.StandardExceptionHandling ) ]
	public class ConsoleCommands
	{
		[ HelpHook, ArgShortcut( "-?" ), ArgDescription( "Shows this help" ) ]
		public bool Help{ get; set; }

		[ ArgActionMethod, ArgDescription( "Adds counter" ) ]
		public void AddCounter( Counter args )
		{
			var counter = new List< string[] >() { new[] { args.Category, args.Name, args.Instance, args.Alias } };
			var counters = PerformanceCounterHelper.GetCounters( counter, null ).ToArray();

			var task = Program.GetSensorTask();
			if( task == null )
				throw new Exception( "Sensor was not started" );
			task.AddCounters( counters );
		}

		[ ArgActionMethod, ArgDescription( "Remove counter" ) ]
		public void RemoveCounter( Counter args )
		{
			var counter = new List< string[] >() { new[] { args.Category, args.Name, args.Instance, args.Alias } };
			var counters = PerformanceCounterHelper.GetCounters( counter, null ).ToArray();

			var task = Program.GetSensorTask();

			Action< string > onRemoved = alias =>
			{
				System.Console.WriteLine( "Removed: {0}", alias );
			};

			task.RemoveCounters( counters, onRemoved );
		}

		[ ArgActionMethod, ArgDescription( "Start Sensor" ) ]
		public void Start( Counters args )
		{
			if( Program.GetSensorsCount() > 1 )
				throw new Exception( "You can't add more then 1 sensor" );

			var counters = GetCountersFromString( args.CountersList, args.GlobalSeparator );
			var sensorObservers = GetDestinationsFromString( args.DestinationsList, args.GlobalSeparator );

			var sensor = new Sensor( 1000, counters );
			sensor.AddObservers( sensorObservers.ToArray() );
			sensor.Start();
			Program.AddSensorTasks( sensor );
		}

		private static IEnumerable< ISensorObserver > GetDestinationsFromString( string destinationsList, string globalSeparator )
		{
			var observersParsed = destinationsList.Split( new string[] { globalSeparator }, StringSplitOptions.None );
			var sensorObservers = observersParsed.Select( x => x.CreateObserver() ).Where( y => y != null );
			return sensorObservers;
		}

		private static Tuple< PerformanceCounter, string >[] GetCountersFromString( string countersList, string globalSeparator )
		{
			var countersSerialized = countersList.Split( new string[] { globalSeparator }, StringSplitOptions.None ).ToList();

			var countersDeserialized = countersSerialized.Select( x =>
			{
				var convert = Args.Convert( x );
				return Args.Parse< Counter >( convert );
			} );

			Action< string, string, string > notifyNotFound = ( x, y, z ) =>
			{
				System.Console.WriteLine( "Not found: {0},{1},{2}", x, y, z );
			};
			var counters = PerformanceCounterHelper.GetCounters( countersDeserialized.Select( x => new[] { x.Category, x.Name, x.Instance, x.Alias }.ToList().Where( y => y != null ).ToArray() ), notifyNotFound ).ToArray();
			return counters;
		}
	}

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