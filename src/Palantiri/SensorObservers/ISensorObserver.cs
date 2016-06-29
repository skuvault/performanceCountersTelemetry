using System;
using System.Collections.Concurrent;
using Palantiri.Counters;

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
			if( string.Equals( observer, "Console", StringComparison.InvariantCultureIgnoreCase ) )
				return new ConsoleObserver();
			else if( string.Equals( observer, "Telegraf", StringComparison.InvariantCultureIgnoreCase ) )
				return new TelegrafObserver();
			else if( string.Equals( observer, "File", StringComparison.InvariantCultureIgnoreCase ) )
				return new FileObserver( args.Length > 0 ? args[ 0 ] : null );
			return null;
		}
	}
}