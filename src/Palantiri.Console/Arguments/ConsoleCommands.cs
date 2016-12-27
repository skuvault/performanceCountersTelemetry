using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Palantiri.Console.Arguments.Parameters;
using Palantiri.Console.Arguments.Parameters.JsonModels;
using Palantiri.Counters;
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
			try
			{
				var counters = GetCounterAndAlias( args, null );
				SensorManager.GetSingleton().GetSensorTask().AddCounters( counters );
			}
			catch( Exception ex )
			{
				Log.Error( "Exception: {@ex}", ex );
				throw;
			}
		}

		[ ArgActionMethod, ArgDescription( "Remove counter" ) ]
		public void RemoveCounter( Counter args )
		{
			try
			{
				var counters = GetCounterAndAlias( args, null );
				SensorManager.GetSingleton().GetSensorTask().RemoveCounters( counters, alias => System.Console.WriteLine( "Removed: {0}", alias ) );
			}
			catch( Exception ex )
			{
				Log.Error( "Exception: {@ex}", ex );
				throw;
			}
		}

		[ ArgActionMethod, ArgDescription( "Create sensor and start" ) ]
		public void CreateSensorAndStart( CreateSensorAndStartParameters args )
		{
			try
			{
				var sensor = CreateSensorFromJson( args );
				sensor.Start();
				SensorManager.GetSingleton().AddSensors( new[] { sensor } );
			}
			catch( Exception ex )
			{
				Log.Error( "Exception: {@ex}", ex );
				throw;
			}
		}

		[ ArgActionMethod, ArgDescription( "Stop Sensor" ) ]
		public void Stop( StopParameters args )
		{
			try
			{
				SensorManager.GetSingleton().GetSensorTask().Stop();
			}
			catch( Exception ex )
			{
				Log.Error( "Exception: {@ex}", ex );
				throw;
			}
		}

		[ ArgActionMethod, ArgDescription( "Start Sensor" ) ]
		public void Start( StartParameters args )
		{
			try
			{
				SensorManager.GetSingleton().GetSensorTask().Start();
			}
			catch( Exception ex )
			{
				Log.Error( "Exception: {@ex}", ex );
				throw;
			}
		}

		private static Sensor CreateSensorFromJson( CreateSensorAndStartParameters args )
		{
			JsonConfig jsonConfig;

			Log.Debug( "Start json parametrs ( " + args.Path + " ) reading..." );
			using( var r = new StreamReader( args.Path, Encoding.UTF8 ) )
			{
				var jsonStr = r.ReadToEnd();
				jsonConfig = JsonConvert.DeserializeObject< JsonConfig >( jsonStr );
			}
			Log.Debug( "End json parametrs ( " + args.Path + " ) reading." );

			Log.Debug( "Start counters parsing..." );
			var counters = jsonConfig.Counters.SelectMany( x => GetCounterAndAlias( x, null ) ).ToArray();
			Log.Debug( "End counters parsing." );

			Log.Debug( "Start observers creation..." );
			var destinations = jsonConfig.Destinations.Select( x =>
			{
				var parameters = string.IsNullOrWhiteSpace( x.Parameters ) ? null : Args.Convert( x.Parameters );
				return x.Name.CreateObserver( parameters );
			} ).ToArray();
			Log.Debug( "End observers creation." );

			var sensor = new Sensor( jsonConfig.Period, jsonConfig.RecreationPeriodMs, counters );
			sensor.AddObservers( destinations );
			return sensor;
		}

		private static PerforrmanceCounterProxy[] GetCounterAndAlias( Counter args, Action<Tuple<CounterFullName, CounterAlias>> onNotFound )
		{
			var counter = new List< Tuple<CounterFullName,CounterAlias> >() { Tuple.Create(new CounterFullName(new CounterName(args.Name), new CounterCategory(args.Category), new CounterInstance(args.Instance)), new CounterAlias(args.Alias)) };
			var counters = PerformanceCounterHelper.GetCounters( counter, onNotFound ).ToArray();
			return counters;
		}
	}
}