using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Palantiri.Console.Arguments.Parameters;
using Palantiri.SensorObservers;
using Palantiri.Sensors;
using PowerArgs;
using Serilog;

namespace Palantiri.Console.Arguments
{
	[ ArgExceptionBehavior( ArgExceptionPolicy.StandardExceptionHandling ) ]
	public class Commands
	{
		[ HelpHook, ArgShortcut( "-?" ), ArgDescription( "Shows this help" ) ]
		public bool Help{ get; set; }

		[ ArgActionMethod, ArgDescription( "Add counter" ) ]
		public void AddCounter( Counter args )
		{
			var counters = GetCounterAndAlias( args, null );
			SensorManager.GetSingleton().GetSensorTask().AddCounters( counters );
		}

		[ ArgActionMethod, ArgDescription( "Remove counter" ) ]
		public void RemoveCounter( Counter args )
		{
			var counters = GetCounterAndAlias( args, null );
			SensorManager.GetSingleton().GetSensorTask().RemoveCounters( counters, alias => System.Console.WriteLine( "Removed: {0}", alias ) );
		}

		[ ArgActionMethod, ArgDescription( "Start Sensor" ) ]
		public void Start( StartParameters args )
		{
			PerformanceCounterHelper.CreateLoggerFromConfig( args.LogLevel, args.LogDestination );
			var counters = GetCountersFromString( args.CountersList, args.GlobalSeparator );
			var sensorObservers = GetDestinationsFromString( args.DestinationsList, args.GlobalSeparator );

			var sensor = new Sensor( 1000, counters );
			sensor.AddObservers( sensorObservers.ToArray() );
			sensor.Start();
			SensorManager.GetSingleton().AddSensors( new[] { sensor } );
		}

		private static Tuple< PerformanceCounter, string >[] GetCountersFromString( string countersString, string globalSeparator )
		{
			var countersStrings = countersString.Split( new[] { globalSeparator }, StringSplitOptions.None ).ToList();

			var countersDeserialized = countersStrings.Select( x =>
			{
				Log.Debug( "Counter string found: {counterstring}", x );
				var convert = Args.Convert( x );
				Log.Debug( "Counter string converted: {convert}", convert );
				var parsedCounter = Args.Parse< Counter >( convert );
				Log.Debug( "Counter string parsed: {@counter}", parsedCounter );
				return parsedCounter;
			} );

			return countersDeserialized.SelectMany( x => GetCounterAndAlias( x, null ) ).ToArray();
		}

		private static Tuple< PerformanceCounter, string >[] GetCounterAndAlias( Counter args, Action< string, string, string > onNotFound )
		{
			var counter = new List< string[] >() { new[] { args.Category, args.Name, args.Instance, args.Alias } };
			var counters = PerformanceCounterHelper.GetCounters( counter, onNotFound ).ToArray();
			return counters;
		}

		private static IEnumerable< ISensorObserver > GetDestinationsFromString( string destinationsList, string globalSeparator )
		{
			var observersParsed = destinationsList.Split( new[] { globalSeparator }, StringSplitOptions.None );
			var sensorObservers = observersParsed.Select( x => x.CreateObserver() ).Where( y => y != null );
			return sensorObservers;
		}
	}
}