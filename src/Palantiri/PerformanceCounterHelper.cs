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

		public static PerformanceCounter GetCounter( string category = "������ CLR .NET", string counterName = "% ������� � GC", string instance = "iisexpress" )
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
			var counterCategory = PerformanceCounterCategory.GetCategories().Where( x => x.CategoryName.Contains( category ) ).First();
			var counter = counterCategory.GetCounters( instance ).Where( x => x.CounterName.Contains( counterName ) ).First();
			return counter;
		}

		public static void WriteLineCounterToConsole( this IDictionary< string, CounterValue > counters )
		{
			WriteLineCounter( counters, Console.WriteLine );
		}

		public static void WriteLineCounter( this IDictionary< string, CounterValue > counters, Action< string > writer )
		{
			foreach( var c in counters )
			{
				writer( string.Format( "[{0}]\t[{1}]\t{2}", c.Value._dateTime, c.Key, c.Value._value ) );
			}
		}
	}
}