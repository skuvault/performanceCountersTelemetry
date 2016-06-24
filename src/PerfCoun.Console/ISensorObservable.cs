using System.Collections.Concurrent;
using PerfCoun.Console.SensorObservers;

namespace PerfCoun.Console
{
	interface ISensorObservable
	{
		void AddObservers( params ISensorObserver[] observers );
		void RemoveObserver( ISensorObserver o );
		void NotifyObservers( ConcurrentDictionary< string, float > counters );
	}
}