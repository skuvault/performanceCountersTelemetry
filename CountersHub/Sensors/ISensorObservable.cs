using System.Collections.Concurrent;
using CountersHub.SensorObservers;

namespace CountersHub.Sensors
{
	interface ISensorObservable
	{
		void AddObservers( params ISensorObserver[] observers );
		void RemoveObserver( ISensorObserver o );
		void NotifyObservers( ConcurrentDictionary< string, float > counters );
	}
}