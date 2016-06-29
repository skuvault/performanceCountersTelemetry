using System;
using Palantiri.Console.Arguments;
using PowerArgs;

namespace Palantiri.Console
{
	class Program
	{
		static void Main( string[] args )
		{
			Args.InvokeAction< Commands >( args );
			System.Console.WriteLine( "Enter 'Exit' key to Exit" );

			string cmd = null;
			while( cmd == null || !cmd.Equals( "Exit", StringComparison.InvariantCultureIgnoreCase ) )
			{
				cmd = ReadUserCommand();

				if( cmd.Equals( "Exit", StringComparison.InvariantCultureIgnoreCase ) )
					return;

				var convert = Args.Convert( cmd );
				Args.InvokeAction< Commands >( convert );
			}
		}

		private static string ReadUserCommand()
		{
			System.Console.Write( ">" );
			var cmd = System.Console.ReadLine();
			return cmd;
		}
	}
}