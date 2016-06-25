using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Palantiri.SensorObservers;

namespace Palantiri.Sensors
{
	public class Sensor: ISensorObservable
	{
		protected Tuple< PerformanceCounter, string >[] _counters{ get; set; }
		protected int _periodMs{ get; set; }
		protected bool _started{ get; set; }
		protected object _startLock = new object();
		protected CancellationTokenSource _sensorCts;
		protected Task _sensorTask;
		protected CancellationToken _sensorCt;
		protected ConcurrentQueue< ConcurrentDictionary< string, float > > _countersQueue;
		protected ConcurrentDictionary< string, string > _countersAlias;
		protected List< ISensorObserver > _observers;

		public void AddObservers( params ISensorObserver[] observers )
		{
			foreach( var sensorObserver in observers )
			{
				this._observers.Add( sensorObserver );
			}
		}

		public void RemoveObserver( ISensorObserver o )
		{
			this._observers.Remove( o );
		}

		public void NotifyObservers( ConcurrentDictionary< string, float > counters )
		{
			foreach( var observer in this._observers )
			{
				observer.SendCounters( counters );
			}
		}

		public Sensor( int periodMs, params Tuple< PerformanceCounter, string >[] counters )
		{
			this._counters = counters;
			this._periodMs = periodMs;
			this._countersQueue = new ConcurrentQueue< ConcurrentDictionary< string, float > >();
			this._observers = new List< ISensorObserver >();
		}

		public static string GetCounterId( PerformanceCounter x )
		{
			return x.CategoryName + "_" + x.CounterName + "_" + x.InstanceName;
		}

		public string GetCounterAlias( PerformanceCounter x )
		{
			string alias;
			if( this._countersAlias.TryGetValue( GetCounterId( x ), out alias ) )
				return alias;
			return null;
		}

		public ConcurrentDictionary< string, float > GetCounterValues()
		{
			var counters = new ConcurrentDictionary< string, float >();
			Parallel.ForEach( this._counters, x =>
			{
				try
				{
					counters.AddOrUpdate( x.Item2, x.Item1.NextValue(), ( cid, y ) => x.Item1.NextValue() );
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
						var counters = this.GetCounterValues();
						this.NotifyObservers( counters );
						Task.Delay( this._periodMs ).Wait( this._sensorCt );
					}
				} );
			}
		}

		public void RemoveCounters( Tuple< PerformanceCounter, string >[] counters, Action< string > onRemoved )
		{
			lock( this._startLock )
			{
				var temp = this._counters.ToList();
				foreach( var counter in counters )
				{
					temp.Remove(counter);
					onRemoved(counter.Item2);
				}
				var tempArray = temp.ToArray();
				this._counters = tempArray;
			}
		}

		public void AddCounters( Tuple< PerformanceCounter, string >[] counters )
		{
			lock( this._startLock )
			{
				var temp = this._counters.ToList();
				temp.AddRange( counters );
				var tempArray = temp.ToArray();
				this._counters = tempArray;
			}
		}
	}
}