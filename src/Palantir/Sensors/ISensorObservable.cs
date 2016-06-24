using System.Collections.Concurrent;
using Palantir.SensorObservers;

namespace Palantir.Sensors
{
	interface ISensorObservable
	{
		void AddObservers( params ISensorObserver[] observers );
		void RemoveObserver( ISensorObserver o );
		void NotifyObservers( ConcurrentDictionary< string, float > counters );
	}
}