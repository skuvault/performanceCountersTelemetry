using System;
using System.Collections.Concurrent;

namespace Palantiri.SensorObservers
{
	public interface ISensorObserver
	{
		void SendCounters( ConcurrentDictionary< CounterAlias, CounterValue > counters );
	}

	public static class ObserverFactory
	{
		public static ISensorObserver CreateObserver( this string observer )
		{
			if( string.Equals( observer, "Console", StringComparison.InvariantCultureIgnoreCase ) )
				return new ConsoleObserver();
			else if( string.Equals( observer, "Telegraf", StringComparison.InvariantCultureIgnoreCase ) )
				return new TelegrafObserver();
			else if( string.Equals( observer, "File", StringComparison.InvariantCultureIgnoreCase ) )
				return new FileObserver();
			return null;
		}
	}
}