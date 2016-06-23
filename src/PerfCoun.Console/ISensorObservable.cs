using System.Collections.Concurrent;
using PerfCoun.Console.SensorClients;

namespace PerfCoun.Console
{
	interface ISensorObservable
	{
		void AddObserver(ISensorObserver o);
		void RemoveObserver(ISensorObserver o);
		void NotifyObservers(ConcurrentDictionary<string, float> counters);
	}
}