using System.Collections.Generic;
using Palantir.Sensors;
using PerfCoun.Console.Arguments;
using PowerArgs;

namespace PerfCoun.Console
{
	class Program
	{
		private static readonly List< Sensor > _sensors = new List< Sensor >();

		static void Main( string[] args )
		{
			Args.InvokeAction< ConsoleCommands >( args );
			System.Console.WriteLine( "Press any key to Exit" );
			System.Console.ReadLine();
		}

		public static void AddSensorTasks( params Sensor[] s )
		{
			_sensors.AddRange( s );
		}
	}
}