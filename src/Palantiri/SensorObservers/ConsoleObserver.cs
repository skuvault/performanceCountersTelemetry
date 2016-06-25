using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Palantiri.SensorObservers
{
	public class ConsoleObserver: ISensorObserver
	{
		protected readonly Task _consoleWriter;
		protected readonly int _period;
		protected readonly int _maxInstancesToProcess;
		protected readonly CancellationTokenSource cts;
		protected readonly CancellationToken ct;
		protected ConcurrentQueue< ConcurrentDictionary< string, float > > _buffer;

		public ConsoleObserver()
		{
			this._buffer = new ConcurrentQueue< ConcurrentDictionary< string, float > >();
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
						ConcurrentDictionary< string, float > res;
						for( var i = 0; i < this._maxInstancesToProcess && this._buffer.TryDequeue( out res ); i++ )
						{
							PerformanceCounterHelper.WriteLineCounter( res );
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

		public void SendCounters( ConcurrentDictionary< string, float > counters )
		{
			this._buffer.Enqueue( counters );
		}
	}
}