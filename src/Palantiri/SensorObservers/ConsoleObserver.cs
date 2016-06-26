using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Palantiri.Sensors;

namespace Palantiri.SensorObservers
{
	public class ConsoleObserver: ISensorObserver
	{
		protected readonly Task _consoleWriter;
		protected readonly int _period;
		protected readonly int _maxInstancesToProcess;
		protected readonly CancellationTokenSource cts;
		protected readonly CancellationToken ct;
		protected ConcurrentQueue< ConcurrentDictionary< string, CounterValue > > _buffer;

		public ConsoleObserver()
		{
			this._buffer = new ConcurrentQueue< ConcurrentDictionary< string, CounterValue > >();
			this._period = 500;
			this.cts = new CancellationTokenSource();
			this.ct = this.cts.Token;
			this._maxInstancesToProcess = 10;

			this._consoleWriter = Task.Factory.StartNew( () =>
			{
				while( !this.ct.IsCancellationRequested )
				{
					if( this._buffer != null )
					{
						ConcurrentDictionary< string, CounterValue > res;
						for( var i = 0; i < this._maxInstancesToProcess && this._buffer.TryDequeue( out res ); i++ )
						{
							res.WriteLineCounterToConsole();
						}
					}
					Task.Delay( this._period ).Wait( this.ct );
				}
			}
				);
		}

		~ConsoleObserver()
		{
			if( this._consoleWriter != null && this.cts != null && this.cts.IsCancellationRequested )
				this.cts.Cancel();
		}

		public void SendCounters( ConcurrentDictionary< string, CounterValue > counters )
		{
			this._buffer.Enqueue( counters );
		}
	}
}