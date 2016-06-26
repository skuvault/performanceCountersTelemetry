using System;
using System.Collections.Concurrent;
using Palantiri.Sensors;

namespace Palantiri.SensorObservers
{
	public interface ISensorObserver
	{
		void SendCounters( ConcurrentDictionary< string, CounterValue > counters );
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