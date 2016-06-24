using System;
using System.Collections.Concurrent;

namespace CountersHub.SensorObservers
{
	public interface ISensorObserver
	{
		void SendCounters( ConcurrentDictionary< string, float > counters );
	}

	public static class ObserverFactory
	{
		public static ISensorObserver CreateObserver( this string observer )
		{
			if( string.Equals( observer, "Console", StringComparison.InvariantCultureIgnoreCase ) )
				return new ConsoleObserver();
			else if( string.Equals( observer, "Telegraf", StringComparison.InvariantCultureIgnoreCase ) )
				return new TelegrafObserver();
			return null;
		}
	}
}