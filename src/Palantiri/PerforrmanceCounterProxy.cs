using System;
using System.Diagnostics;
using Palantiri.Counters;

namespace Palantiri
{
	public class PerforrmanceCounterProxy
	{
		private PerformanceCounter _performanceCounter;

		public PerforrmanceCounterProxy( PerformanceCounter counter, CounterFullName fname,  string @alias )
		{
			this._performanceCounter = counter;
			this.Alias = alias;
			this.FullName = fname;
		}


		public PerforrmanceCounterProxy( PerformanceCounter counter, CounterFullName fname ) : this( counter, fname, null )
		{
		}

		public void ReFresh()
		{
			try
			{
				lock (this)
				{
					this._performanceCounter = PerformanceCounterHelper.GetCounter( this.FullName );
				}
			}
			catch ( Exception ex )
			{
				Serilog.Log.Error( ex, "Performance Counter {categoryName}//{counterName}//{instanceName} can't be refreshed", FullName.Category, FullName.Name, FullName.Instance );
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

		public CounterFullName FullName { get; private set; }

		public string Alias{ get; private set; }

		public override string ToString()
		{
			return string.Format( "{{Counter: {0}, Alias: {1}}}", Counter.ToString(), Alias );
		}
	}
}