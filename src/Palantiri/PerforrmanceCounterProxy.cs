using System;
using System.Diagnostics;

namespace Palantiri
{
	public class PerforrmanceCounterProxy
	{
		private PerformanceCounter _performanceCounter;

		public PerforrmanceCounterProxy( PerformanceCounter counter, string @alias )
		{
			this._performanceCounter = counter;
			this.Alias = alias;
		}

		public PerforrmanceCounterProxy( PerformanceCounter counter ): this( counter, null )
		{
		}

		public void ReFresh()
		{
			var instanceName = string.Empty;
			var counterName = string.Empty;
			var categoryName = string.Empty;
			try
			{
				lock (this)
				{
					categoryName = this._performanceCounter.CategoryName;
					counterName = this._performanceCounter.CounterName;
					instanceName = this._performanceCounter.InstanceName;
					this._performanceCounter = PerformanceCounterHelper.GetCounter(categoryName, counterName, instanceName);
				}
			}
			catch ( Exception ex )
			{
				Serilog.Log.Error( ex, "Performance Counter {categoryName}//{counterName}//{instanceName} can't be refreshed", categoryName, counterName, instanceName );
			}
		}

		public PerformanceCounter Counter
		{
			get
			{
				lock ( this )
				{
					return _performanceCounter;
				}
			}
		}

		public string Alias{ get; private set; }

		public override string ToString()
		{
			return string.Format( "{{Counter: {0}, Alias: {1}}}", Counter.ToString(), Alias );
		}
	}
}