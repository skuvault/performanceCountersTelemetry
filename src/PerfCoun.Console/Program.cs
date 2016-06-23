using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerfCoun.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			var s = new Sensor(500, PerformanceCounterHelper.GetCounter());
			s.Start();
			System.Console.ReadLine();
			s.Stop();
		}
	}
}
