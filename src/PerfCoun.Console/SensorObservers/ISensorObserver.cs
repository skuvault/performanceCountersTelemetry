using System.Collections.Concurrent;

namespace PerfCoun.Console.SensorClients
{
	public interface ISensorObserver
	{
		void SendCounters( ConcurrentDictionary< string, float > counters );
	}
}