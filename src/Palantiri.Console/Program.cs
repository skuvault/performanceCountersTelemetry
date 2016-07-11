using System;
using System.Reflection;
using Palantiri.Console.Arguments;
using PowerArgs;
using Serilog;

namespace Palantiri.Console
{
	class Program
	{
		static int Main( string[] args )
		{
			Log.Information( "Start Palantiri ver:" + Assembly.GetExecutingAssembly().GetName().Version );
			Args.InvokeAction< Commands >( args );
			System.Console.WriteLine( "Enter 'Exit' key to Exit" );

			string cmd = null;
			while( cmd == null || !cmd.Equals( "Exit", StringComparison.InvariantCultureIgnoreCase ) )
			{
				cmd = ReadUserCommand();

				if( cmd.Equals( "Exit", StringComparison.InvariantCultureIgnoreCase ) )
					return 0;

				var convert = Args.Convert( cmd );
				Args.InvokeAction< Commands >( convert );
			}

			return 1;
		}

		private static string ReadUserCommand()
		{
			System.Console.Write( ">" );
			var cmd = System.Console.ReadLine();
			return cmd;
		}
	}
}