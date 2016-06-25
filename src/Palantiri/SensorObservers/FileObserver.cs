using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Palantiri.Properties;

namespace Palantiri.SensorObservers
{
	public class FileObserver: ISensorObserver
	{
		protected readonly Task _consoleWriter;
		protected readonly int _period;
		protected readonly int _maxInstancesToProcess;
		protected readonly CancellationTokenSource cts;
		protected readonly CancellationToken ct;
		protected ConcurrentQueue< ConcurrentDictionary< string, float > > _buffer;
		private StreamWriter _file;

		public FileObserver()
		{
			this._buffer = new ConcurrentQueue< ConcurrentDictionary< string, float > >();
			this._period = 500;
			this.cts = new CancellationTokenSource();
			this.ct = this.cts.Token;
			this._maxInstancesToProcess = 10;

			this._consoleWriter = Task.Factory.StartNew( () =>
			{
				this._file = new StreamWriter( Settings.Default.FileOberverPath );
				while( !this.ct.IsCancellationRequested )
				{
					if( this._buffer != null )
					{
						ConcurrentDictionary< string, float > res;
						for( var i = 0; i < this._maxInstancesToProcess && this._buffer.TryDequeue( out res ); i++ )
						{
							PerformanceCounterHelper.WriteLineCounter( res, x => this._file.WriteLine( x ) );
							this._file.Flush();
						}
					}
					Task.Delay( this._period ).Wait( this.ct );
				}
			}
				);
		}

		~FileObserver()
		{
			if( this._consoleWriter != null && this.cts != null && this.cts.IsCancellationRequested )
				this.cts.Cancel();
			this._file.Close();
		}

		public void SendCounters( ConcurrentDictionary< string, float > counters )
		{
			this._buffer.Enqueue( counters );
		}
	}
}