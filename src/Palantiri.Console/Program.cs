using System;
using System.Collections.Generic;
using System.Linq;
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

				if (cmd.Equals("Exit", StringComparison.InvariantCultureIgnoreCase))
					return;

				var convert = Args.Convert(cmd);
				Args.InvokeAction<ConsoleCommands>(convert);
				

			}
		}


		public static void AddSensorTasks( params Sensor[] s )
		{
			_sensors.AddRange( s );
		}

		public static Sensor GetSensorTask()
		{
			return _sensors.First();
		}
	}

	public static class Helper
	{

		public static IEnumerable<string> SplitAsArgs(this string str,
										  Func<char, bool> controller)
		{
			int nextPiece = 0;

			for (int c = 0; c < str.Length; c++)
			{
				if (controller(str[c]))
				{
					yield return str.Substring(nextPiece, c - nextPiece);
					nextPiece = c + 1;
				}
			}

			yield return str.Substring(nextPiece);
		}
	}
}