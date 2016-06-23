using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PerfCoun.Console
{
	public class Sensor
	{
		protected PerformanceCounter[] _counters{ get; set; }
		protected int _periodMs{ get; set; }
		protected bool _started{ get; set; }
		protected object _startLock = new object();
		protected CancellationTokenSource _sensorCts;
		protected Task _sensorTask;
		protected CancellationToken _sensorCt;
		protected ConcurrentQueue< ConcurrentDictionary< string, float > > countersQueue{ get; set; }

		public Sensor( int periodMs, params PerformanceCounter[] counters )
		{
			this._counters = counters;
			this._periodMs = periodMs;
			this.countersQueue = new ConcurrentQueue< ConcurrentDictionary< string, float > >();
		}

		public static string GetCounterId( PerformanceCounter x )
		{
			return x.CategoryName + "_" + x.CounterName + "_" + x.InstanceName;
		}

		public ConcurrentDictionary< string, float > GetCounters()
		{
			var counters = new ConcurrentDictionary< string, float >();
			Parallel.ForEach( this._counters, x =>
			{
				try
				{
					counters.AddOrUpdate( GetCounterId( x ), x.NextValue(), ( cid, y ) => x.NextValue() );
				}
				catch( Exception )
				{
					throw; //todo notify
				}
			} );
			return counters;
		}

		public void Stop()
		{
			lock( this._startLock )
			{
				this._sensorCts.Cancel();
				this._started = false;
			}
		}

		public void Start()
		{
			lock( this._startLock )
			{
				if( this._started )
					return;

				this._started = true;
				this._sensorCts = new CancellationTokenSource();
				this._sensorCt = this._sensorCts.Token;
				this._sensorTask = Task.Factory.StartNew( () =>
				{
					while( this._started && !this._sensorCt.IsCancellationRequested )
					{
						var counters = this.GetCounters();
						//countersQueue.Enqueue(counters);//todo: to reader/ writer
						PerformanceCounterHelper.WriteLineCounter( counters ); //replace to reader writer;
						Task.Delay( this._periodMs ).Wait( this._sensorCt );
					}
				} );
			}
		}
	}
}