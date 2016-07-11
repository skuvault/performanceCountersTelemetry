using System;
using System.Collections.Concurrent;
using Palantiri.Counters;
using Serilog;

namespace Palantiri.SensorObservers
{
	public interface ISensorObserver
	{
		void SendCounters( ConcurrentDictionary< CounterAlias, CounterValue > counters );
	}

	public static class ObserverFactory
	{
		public static ISensorObserver CreateObserver( this string observer, params string[] args )
		{
			Log.Debug( "Start observer creation..." );
			ISensorObserver consoleObserver = null;

			if( string.Equals( observer, "Console", StringComparison.InvariantCultureIgnoreCase ) )
				consoleObserver = new ConsoleObserver();
			else if( string.Equals( observer, "Telegraf", StringComparison.InvariantCultureIgnoreCase ) )
				consoleObserver = new TelegrafObserver( args );
			else if( string.Equals( observer, "File", StringComparison.InvariantCultureIgnoreCase ) )
				consoleObserver = new FileObserver( args.Length > 0 ? args[ 0 ] : null );

			Log.Debug( "End observer creation." );
			return consoleObserver;
		}
	}
}