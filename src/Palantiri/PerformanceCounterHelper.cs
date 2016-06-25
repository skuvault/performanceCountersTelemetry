using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Palantiri.Sensors;

namespace Palantiri
{
	public static class PerformanceCounterHelper
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="counters">[Category,Instance,CounterName]</param>
		/// <param name="onNotFound"></param>
		/// <returns></returns>
		public static IEnumerable< Tuple< PerformanceCounter, string > > GetCounters( IEnumerable< string[] > counters, Action< string, string, string > onNotFound )
		{
			foreach( var counterNameAndAlias in counters )
			{
				var performanceCounter = GetCounter( counterNameAndAlias[ 0 ], counterNameAndAlias[ 1 ], counterNameAndAlias[ 2 ] );

				if( performanceCounter == null && onNotFound != null )
					onNotFound( counterNameAndAlias[ 0 ], counterNameAndAlias[ 1 ], counterNameAndAlias[ 2 ] );

				string alias = null;
				if( performanceCounter != null )
				{
					alias = Sensor.GetCounterId( performanceCounter );
					if( counterNameAndAlias.Length > 3 && !string.IsNullOrWhiteSpace( counterNameAndAlias[ 3 ] ) )
						alias = counterNameAndAlias[ 3 ];
				}

				if( performanceCounter != null )
					yield return Tuple.Create( performanceCounter, alias );
			}
		}

		public static PerformanceCounter GetCounter( string category = "Память CLR .NET", string counterName = "% времени в GC", string instance = "iisexpress" )
		{
			PerformanceCounter res = null;
			var performanceCounterCategories = PerformanceCounterCategory.GetCategories().Where( x => x.CategoryName == category ).ToList();
			foreach( var performanceCounterCategory in performanceCounterCategories )
			{
				if( performanceCounterCategory.CategoryType != PerformanceCounterCategoryType.SingleInstance )
				{
					var instanceNames = performanceCounterCategory.GetInstanceNames();
					var targetInstances = instanceNames.Where( x => x == instance ).ToList();
					foreach( var targetInstance in targetInstances )
					{
						var performanceCounters = performanceCounterCategory.GetCounters( targetInstance ).Where( y => y.CategoryName == category );
						foreach( var counter in performanceCounters )
						{
							if( counter.CounterName == counterName )
							{
								Console.WriteLine( "Found: {0},{1},{2}", category, instance, counterName );
								res = counter;
							}
						}
					}
				}
				else
				{
					foreach( var counter in performanceCounterCategory.GetCounters() )
					{
						Console.WriteLine( ( string )counter.CounterName );
					}
				}
			}
			return res;
		}

		public static PerformanceCounter GetCounter2( string category = ".NET CLR Memory", string instance = "_Global_", string counterName = "% Time in GC" )
		{
			var counterCategory = Enumerable.First( Enumerable.Where( PerformanceCounterCategory.GetCategories(), x => x.CategoryName.Contains( category ) ) );
			var counter = Enumerable.First( Enumerable.Where( counterCategory.GetCounters( instance ), x => x.CounterName.Contains( counterName ) ) );
			return counter;
		}

		public static void WriteLineCounter( this IDictionary< string, float > counters )
		{
			foreach( var cc in counters )
			{
				Console.WriteLine( string.Format( "{0}={1}", cc.Key, cc.Value ) );
			}
		}
	}
}