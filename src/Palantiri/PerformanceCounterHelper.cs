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
			CreateLoggerFromConfig();
		}

		public static void CreateLoggerFromConfig()
		{
			var logger = new LoggerConfiguration()
				.ReadFrom.AppSettings()
				.CreateLogger();
			Log.Logger = logger;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="counters">[Category,Instance,CounterName]</param>
		/// <param name="onNotFound"></param>
		/// <returns></returns>
		public static IEnumerable<PerforrmanceCounterProxy> GetCounters(IEnumerable<string[]> counters, Action<string, string, string> onNotFound)
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

				yield return new PerforrmanceCounterProxy(performanceCounter, alias);
			}

			Log.Debug( "Counters received" );
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
					var instanceCounters = GetCountersOrNull( instance, counterCategory );

					if (instanceCounters != null)
					{
						var performanceCounters = instanceCounters.FirstOrDefault(y => string.Equals(y.CounterName, counterName, StringComparison.InvariantCultureIgnoreCase));
						if (performanceCounters == null)
							Log.Warning("Counter {name}({instance}) not found in category {category}", counterName, instance, category);
						else
							res = performanceCounters;
					}
					else
					{
						Log.Warning("Counter {name}({instance}) not found in category {category}", counterName, instance, category);
					}
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

		private static PerformanceCounter[] GetCountersOrNull( string instance, PerformanceCounterCategory counterCategory )
		{
			PerformanceCounter[] instanceCounters;
			try
			{
				instanceCounters = counterCategory.GetCounters( instance );
			}
			catch
			{
				instanceCounters = null;
			}
			return instanceCounters;
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