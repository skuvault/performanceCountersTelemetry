using System;
using System.Collections.Concurrent;
using Palantiri.SensorObservers;

namespace Palantiri.Sensors
{
	interface ISensorObservable
	{
		void AddObservers( params ISensorObserver[] observers );
		void RemoveObserver( ISensorObserver o );
		void NotifyObservers( ConcurrentDictionary< string, Tuple< DateTime, float > > counters );
	}
}