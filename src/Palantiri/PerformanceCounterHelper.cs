using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Palantiri.Sensors;
using Serilog;

namespace Palantiri
{
	public static class PerformanceCounterHelper
	{
		static PerformanceCounterHelper()
		{
			CreateLoggerFromConfig( null, null );
		}

		public static void CreateLoggerFromConfig( string logLevel, string destination )
		{
			Log.Logger = new LoggerConfiguration()
				.SetLogConfig( logLevel, destination )
				.CreateLogger();
		}

		public static LoggerConfiguration SetLogConfig( this LoggerConfiguration lc, string logLevel, string destination )
		{
			lc.SetLogLevel( logLevel );
			lc.SetLogDestination( destination );
			return lc;
		}

		public static LoggerConfiguration SetLogLevel( this LoggerConfiguration lc, string logLevel )
		{
			if( string.Equals( logLevel, "Information", StringComparison.InvariantCultureIgnoreCase ) )
				lc.MinimumLevel.Information();
			else if( string.Equals( logLevel, "Warning", StringComparison.InvariantCultureIgnoreCase ) )
				lc.MinimumLevel.Warning();
			else if( string.Equals( logLevel, "Error", StringComparison.InvariantCultureIgnoreCase ) )
				lc.MinimumLevel.Error();
			else if( string.Equals( logLevel, "Debug", StringComparison.InvariantCultureIgnoreCase ) )
				lc.MinimumLevel.Debug();
			else
				lc.MinimumLevel.Warning();
			return lc;
		}

		public static LoggerConfiguration SetLogDestination( this LoggerConfiguration lc, string destination )
		{
			if( string.Equals( destination, "ColoredConsole", StringComparison.InvariantCultureIgnoreCase ) )
				lc.WriteTo.ColoredConsole();
			else if( string.Equals( destination, "File", StringComparison.InvariantCultureIgnoreCase ) )
				lc.WriteTo.RollingFile( @"Log-{Date}.txt" );
			return lc;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="counters">[Category,Instance,CounterName]</param>
		/// <param name="onNotFound"></param>
		/// <returns></returns>
		public static IEnumerable< Tuple< PerformanceCounter, string > > GetCounters( IEnumerable< string[] > counters, Action< string, string, string > onNotFound )
		{
			Log.Debug( "Start getting counters..." );

			foreach( var counterNameAndAlias in counters )
			{
				var performanceCounter = GetCounter( counterNameAndAlias[ 0 ], counterNameAndAlias[ 1 ], counterNameAndAlias[ 2 ] );

				if( performanceCounter == null )
				{
					if( onNotFound != null )
						onNotFound( counterNameAndAlias[ 0 ], counterNameAndAlias[ 1 ], counterNameAndAlias[ 2 ] );
					continue;
				}

				var alias = Sensor.GetCounterId( performanceCounter );
				if( counterNameAndAlias.Length > 3 && !string.IsNullOrWhiteSpace( counterNameAndAlias[ 3 ] ) )
					alias = counterNameAndAlias[ 3 ];

				yield return Tuple.Create( performanceCounter, alias );
			}

			Log.Debug( "Counters received" );
		}

		public static PerformanceCounter GetCounterOld( string category, string counterName, string instance )
		{
			Log.Information( "Getting counter: {category}\\{name}\\{instance} ", category, counterName, instance );
			PerformanceCounter res = null;
			var performanceCounterCategories = PerformanceCounterCategory.GetCategories().Where( x => String.Equals( x.CategoryName, category, StringComparison.InvariantCultureIgnoreCase ) ).ToList();
			Log.Debug( "Suitable categories found: {@performanceCounterCategories}", performanceCounterCategories );
			foreach( var performanceCounterCategory in performanceCounterCategories )
			{
				if( performanceCounterCategory.CategoryType != PerformanceCounterCategoryType.SingleInstance )
				{
					var instanceNames = performanceCounterCategory.GetInstanceNames();
					var targetInstances = instanceNames.Where( x => String.Equals( x, instance, StringComparison.InvariantCultureIgnoreCase ) ).ToList();
					Log.Debug( "Suitable instanceNames found: {@targetInstances}", targetInstances );

					foreach( var targetInstance in targetInstances )
					{
						var performanceCounters = performanceCounterCategory.GetCounters( targetInstance ).Where( y => String.Equals( y.CategoryName, category, StringComparison.InvariantCultureIgnoreCase ) );
						Log.Debug( "Suitable counters found: {@performanceCounters}", performanceCounters );
						foreach( var counter in performanceCounters )
						{
							if( counter.CounterName == counterName )
								res = counter;
						}
					}
				}
				else
				{
					//foreach( var counter in performanceCounterCategory.GetCounters() )
					//{
					//	Console.WriteLine( ( string )counter.CounterName );
					//}
				}
			}

			if( res == null )
				Log.Warning( "Counter not found: {category}\\{name}\\{instance}", category, counterName, instance );
			else
				Log.Information( "Counter found: {category}\\{name}\\{instance}", category, counterName, instance );
			return res;
		}

		public static PerformanceCounter GetCounter( string category, string counterName, string instance )
		{
			Log.Information( "Getting counter: {category}\\{name}\\{instance} ", category, counterName, instance );
			PerformanceCounter res = null;
			var counterCategory = PerformanceCounterCategory.GetCategories().FirstOrDefault( x => string.Equals( x.CategoryName, category, StringComparison.InvariantCultureIgnoreCase ) );
			if( counterCategory != null )
			{
				if( counterCategory.CategoryType == PerformanceCounterCategoryType.MultiInstance )
				{
					var instanceCounters = counterCategory.GetCounters( instance );
					var performanceCounters = instanceCounters.FirstOrDefault( y => string.Equals( y.CounterName, counterName, StringComparison.InvariantCultureIgnoreCase ) );
					if( performanceCounters == null )
						Log.Warning( "Counter {name}({instance}) not found in category {category}", counterName, instance, category );
					else
						res = performanceCounters;
				}
			}
			else
				Log.Warning( "Category not found: {category}", category );
			if( res == null )
				Log.Warning( "Counter not found: {category}\\{name}\\{instance}", category, counterName, instance );
			else
				Log.Information( "Counter found: {category}\\{name}\\{instance}", category, counterName, instance );
			return res;
		}

		public static PerformanceCounter GetCounter2( string category , string instance , string counterName )
		{
			var counterCategory = PerformanceCounterCategory.GetCategories().Where( x => x.CategoryName.Contains( category ) ).First();
			var counter = counterCategory.GetCounters( instance ).Where( x => x.CounterName.Contains( counterName ) ).First();
			return counter;
		}

		public static void WriteLineCounterToConsole( this IDictionary< CounterAlias, CounterValue > counters )
		{
			WriteLineCounter( counters, Console.WriteLine );
		}

		public static void WriteLineCounter( this IDictionary< CounterAlias, CounterValue > counters, Action< string > writer )
		{
			foreach( var c in counters )
			{
				writer( string.Format( "[{0}]\t[{1}]\t{2}", c.Value.DateTime.ToString( "yyyy.MM.dd HH:mm:ss.fff" ), c.Key.Alias, c.Value.Value ) );
			}
		}
	}
}