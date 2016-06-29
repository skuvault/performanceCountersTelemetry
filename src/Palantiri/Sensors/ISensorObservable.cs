using System.Collections.Concurrent;
using Palantiri.Counters;
using Palantiri.SensorObservers;

namespace Palantiri.Sensors
{
	interface ISensorObservable
	{
		void AddObservers( params ISensorObserver[] observers );
		void RemoveObserver( ISensorObserver o );
		void NotifyObservers( ConcurrentDictionary< CounterAlias, CounterValue > counters );
	}
}