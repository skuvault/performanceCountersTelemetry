using System;
using System.Collections.Generic;
using Palantiri.Console.Arguments;
using Palantiri.Sensors;
using PowerArgs;

namespace Palantiri.Console
{
	class Program
	{
		private static readonly List< Sensor > _sensors = new List< Sensor >();

		static void Main( string[] args )
		{
			Args.InvokeAction< ConsoleCommands >( args );
			System.Console.WriteLine( "Enter 'Exit' key to Exit" );
			var cmd = "";
			while( cmd == null || !cmd.Equals( "Exit", StringComparison.InvariantCultureIgnoreCase ) )
			{

				cmd = System.Console.ReadLine();
			}
		}

		public static void AddSensorTasks( params Sensor[] s )
		{
			_sensors.AddRange( s );
		}
	}
}