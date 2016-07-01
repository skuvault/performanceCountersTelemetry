using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Palantiri.Counters;
using Serilog;

namespace Palantiri.SensorObservers
{
	public class FileObserver: ISensorObserver
	{
		protected readonly Task _consoleWriter;
		protected readonly int _period;
		protected readonly int _maxInstancesToProcess;
		protected readonly CancellationTokenSource cts;
		protected readonly CancellationToken ct;
		protected ConcurrentQueue< ConcurrentDictionary< CounterAlias, CounterValue > > _buffer;
		protected int _bufferDrainLimit = 100;

		private StreamWriter _file;
		private string _fileName = "palantiri-output.txt";

		public FileObserver( string fileName )
		{
			this._buffer = new ConcurrentQueue< ConcurrentDictionary< CounterAlias, CounterValue > >();
			this._period = 500;
			this.cts = new CancellationTokenSource();
			this.ct = this.cts.Token;
			this._maxInstancesToProcess = 10;

			this._consoleWriter = Task.Factory.StartNew( () =>
			{
				if( !string.IsNullOrWhiteSpace( fileName ) )
					this._fileName = fileName;

				this._file = new StreamWriter( this._fileName );
				while( !this.ct.IsCancellationRequested )
				{
					Log.Debug( "Start FileObserver listening..." );
					if( this._buffer != null )
					{
						ConcurrentDictionary< CounterAlias, CounterValue > res;

						var overflow = this._buffer.Count - this._bufferDrainLimit;
						if( overflow > 0 )
						{
							for( var j = 0; j < overflow; j++ )
							{
								this._buffer.TryDequeue( out res );
							}
						}

						for( var i = 0; i < this._maxInstancesToProcess && this._buffer.TryDequeue( out res ); i++ )
						{
							Log.Debug( "Start FileObserver values sending." );
							res.WriteLineCounter( x => this._file.WriteLine( x ) );
							this._file.Flush();
							Log.Debug( "FileObserver values sent successfully." );
						}
					}
					Log.Debug( "End FileObserver listening." );
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

		public void SendCounters( ConcurrentDictionary< CounterAlias, CounterValue > counters )
		{
			this._buffer.Enqueue( counters );
		}
	}
}