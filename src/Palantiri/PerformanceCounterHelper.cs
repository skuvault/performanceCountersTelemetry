using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Palantiri.Counters;
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
		public static IEnumerable< PerforrmanceCounterProxy > GetCounters( List<Tuple<CounterFullName, CounterAlias>> counters, Action<Tuple<CounterFullName, CounterAlias>> onNotFound )
		{
			Log.Debug( "Start getting counters..." );

			foreach( var counterNameAndAlias in counters )
			{
				var counterFullName = counterNameAndAlias.Item1;
				var performanceCounter = GetCounter( counterFullName);

				if( performanceCounter == null )
				{
					onNotFound?.Invoke(counterNameAndAlias);
					yield return new PerforrmanceCounterProxy(performanceCounter, counterFullName, counterNameAndAlias.Item2);
				}
				else
				{
					var alias = counterNameAndAlias.Item2 != null && !counterNameAndAlias.Item2.IsEmpty()
						? counterNameAndAlias.Item2
						: new CounterAlias(Sensor.GetCounterId(performanceCounter));

					yield return new PerforrmanceCounterProxy(performanceCounter, counterFullName, alias);
				}
			}

			Log.Debug( "Counters received" );
		}

		public static PerformanceCounter GetCounter( CounterFullName cntrFullName )
		{
			Log.Information( "Getting counter: {category}\\{name}\\{instance} ", cntrFullName.Category.Categpry, cntrFullName.Name.Name, cntrFullName.Instance.Instance );
			PerformanceCounter res = null;
			var counterCategory = PerformanceCounterCategory.GetCategories().FirstOrDefault( x => string.Equals( x.CategoryName, cntrFullName.Category.Categpry, StringComparison.InvariantCultureIgnoreCase ) );
			if( counterCategory != null )
			{
				if( counterCategory.CategoryType == PerformanceCounterCategoryType.MultiInstance )
				{
					var instanceCounters = GetCountersOrNull( cntrFullName.Instance.Instance, counterCategory );

					if( instanceCounters != null )
					{
						var performanceCounters = instanceCounters.FirstOrDefault( y => string.Equals( y.CounterName, cntrFullName.Name.Name, StringComparison.InvariantCultureIgnoreCase ) );
						if( performanceCounters == null )
							Log.Warning( "Counter {name}({instance}) not found in category {category}", cntrFullName.Name.Name, cntrFullName.Instance.Instance, cntrFullName.Category.Categpry );
						else
							res = performanceCounters;
					}
					else
						Log.Warning( "Counter {name}({instance}) not found in category {category}", cntrFullName.Name.Name, cntrFullName.Instance.Instance, cntrFullName.Category.Categpry );
				}
				else if( counterCategory.CategoryType == PerformanceCounterCategoryType.SingleInstance )
				{
					var instanceCounters = GetCountersOrNull( null, counterCategory );

					if( instanceCounters != null )
					{
						var performanceCounters = instanceCounters.FirstOrDefault( y => string.Equals( y.CounterName, cntrFullName.Name.Name, StringComparison.InvariantCultureIgnoreCase ) );
						if( performanceCounters == null )
							Log.Warning( "Counter {name} not found in category {category}", cntrFullName.Name.Name, cntrFullName.Category.Categpry );
						else
							res = performanceCounters;
					}
					else
						Log.Warning( "Counter {name} not found in category {category}", cntrFullName.Name.Name, cntrFullName.Category.Categpry );
				}
			}
			else
				Log.Warning( "Category not found: {category}", cntrFullName.Category.Categpry );

			if( res == null )
				Log.Warning( "Counter not found: {category}\\{name}\\{instance}", cntrFullName.Category.Categpry, cntrFullName.Name.Name, cntrFullName.Instance.Instance );
			else
				Log.Information( "Counter found: {category}\\{name}\\{instance}", cntrFullName.Category.Categpry, cntrFullName.Name.Name, cntrFullName.Instance.Instance );
			return res;
		}

		private static PerformanceCounter[] GetCountersOrNull( string instance, PerformanceCounterCategory counterCategory )
		{
			PerformanceCounter[] instanceCounters;
			try
			{
				instanceCounters = !string.IsNullOrWhiteSpace( instance ) ? counterCategory.GetCounters( instance ) : counterCategory.GetCounters();
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