using PerfCoun.Console.SensorClients;
using PerfCoun.Console.SensorObservers;

namespace PerfCoun.Console
{
	class Program
	{
		static void Main( string[] args )
		{
			var s = new Sensor( 1000, PerformanceCounterHelper.GetCounter() );
			s.AddObservers(new ConsoleObserver());

			s.Start();
			System.Console.ReadLine();
			s.Stop();
		}
	}
}