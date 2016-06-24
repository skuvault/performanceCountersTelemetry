using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PerfCoun.Console
{
	public static class PerformanceCounterHelper
	{
		public static PerformanceCounter GetCounter4( string category = "Память CLR .NET", string instance = "_Global_", string counterName = "% времени в GC" )
		{
			var counterCategory = Enumerable.First( Enumerable.Where( PerformanceCounterCategory.GetCategories(), x => x.CategoryName.Contains( category ) ) );
			var counter = Enumerable.First( Enumerable.Where( counterCategory.GetCounters( instance ), x => x.CounterName.Contains( counterName ) ) );
			return counter;
		}

		public static IEnumerable< PerformanceCounter > GetCounters( List< string[] > counters, Action< string, string, string > notFound )
		{
			foreach( var stringse in counters )
			{
				var performanceCounter = GetCounter( stringse[ 0 ], stringse[ 1 ], stringse[ 2 ] );
				if( performanceCounter == null )
					notFound( stringse[ 0 ], stringse[ 1 ], stringse[ 2 ] );
				yield return performanceCounter;
			}
		}

		public static PerformanceCounter GetCounter( string category = "Память CLR .NET", string instance = "iisexpress", string counterName = "% времени в GC" )
		{
			PerformanceCounter res = null;
			var performanceCounterCategories = PerformanceCounterCategory.GetCategories().Where( x => x.CategoryName.Contains( "Память" ) && x.CategoryName.Contains( "NET" ) ).ToList();
			foreach( PerformanceCounterCategory category2 in performanceCounterCategories )
			{
				if( category2.CategoryType != PerformanceCounterCategoryType.SingleInstance )
				{
					string[] names = category2.GetInstanceNames();
					var enumerable = names.Where( x => x.Contains( "iis" ) && !x.Contains( "tray" ) ).ToList();
					foreach( string name in enumerable )
					{
						var performanceCounters = category2.GetCounters( name ).Where( y => y.CategoryName.Contains( ".NET" ) );
						foreach( var counter in performanceCounters )
						{
							if( counter.CounterName.Contains( "времени" ) )
							{
								System.Console.WriteLine( counter.CounterName );
								res = counter;
							}
						}
					}
				}
				else
				{
					foreach( var counter in category2.GetCounters() )
					{
						System.Console.WriteLine( counter.CounterName );
					}
				}
			}
			return res;
			//
			//	var performanceCounterCategories = PerformanceCounterCategory.GetCategories();
			//	var counterCategories = performanceCounterCategories.Where( x => x.CategoryName.Contains(category));
			//	var counterCategory = counterCategories.First();
			//	var counters = counterCategory.GetCounters("");
			//	var performanceCounters = counters.Where(y=>y.CounterName =="% времени в GC" );
			//	var performanceCounters2 = counterCategory.GetCounters("iisexpress").Where(y => y.InstanceName == "iisexpress");
			//	var counter = Enumerable.Where(performanceCounters, x => x.CounterName.Contains(counterName)).First();
			//	return counter;
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
				System.Console.WriteLine( string.Format( "{0}={1}", cc.Key, cc.Value ) );
			}
		}
	}
}