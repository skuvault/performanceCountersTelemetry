using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Palantiri.Counters;
using Serilog;
using Telegraf;

namespace Palantiri.SensorObservers
{
	public class TelegrafObserver: ISensorObserver
	{
		protected Task _consoleWriter;
		protected readonly int _period;
		protected readonly int _maxInstancesToProcess;
		protected readonly CancellationTokenSource cts;
		protected readonly CancellationToken ct;
		protected ConcurrentQueue< ConcurrentDictionary< CounterAlias, CounterValue > > _buffer;
		protected readonly int _bufferDrainLimit;

		public TelegrafObserver( params string[] args )
		{
			Log.Debug( "Start TelegrafObserver creation..." );
			SetupStatistics.Init( args[ 2 ], args[ 3 ], args[ 4 ] );
			this._buffer = new ConcurrentQueue< ConcurrentDictionary< CounterAlias, CounterValue > >();
			this._period = int.Parse( args[ 0 ] );
			this.cts = new CancellationTokenSource();
			this.ct = this.cts.Token;
			this._maxInstancesToProcess = 10;
			this._bufferDrainLimit = int.Parse( args[ 1 ] );
			this.StartListening();
			Log.Debug( "TelegrafObserver created." );
		}

		protected void StartListening()
		{
			this._consoleWriter = Task.Factory.StartNew( () =>
			{
				while( !this.ct.IsCancellationRequested )
				{
					this.Listen();
					Task.Delay( this._period, this.ct ).Wait();
				}
			}, this.ct );
		}

		protected void Listen()
		{
			Log.Debug( "Start TelegrafObserver listening..." );
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

				var valuesinBuffer = this._buffer.Count;
				for( var i = 0; i < this._maxInstancesToProcess && i < valuesinBuffer; i++ )
				{
					this._buffer.TryDequeue( out res );
					Log.Debug( "Start TelegrafObserver get {valuenum} value from queue({queuelen}).", i, valuesinBuffer );
					var values = res.ToDictionary( x => x.Key.Alias, y => ( object )y.Value.Value );
					if( values.Any() )
					{
						Log.Debug( "Start TelegrafObserver values sending... ({values})", JsonConvert.SerializeObject( values ) );
						Metrics.Record( "app-sys-counters", values );
						Log.Debug( "TelegrafObserver values sent successfully." );
					}
					else
						Log.Debug( "TelegrafObserver there are no values." );
				}
			}
			Log.Debug( "End TelegrafObserver listening." );
		}

		~TelegrafObserver()
		{
			if( this._consoleWriter != null && this.cts != null && this.cts.IsCancellationRequested )
				this.cts.Cancel();
		}

		public void SendCounters( ConcurrentDictionary< CounterAlias, CounterValue > counters )
		{
			Log.Debug( "Start TelegrafObserver receiving  notification..." );
			this._buffer.Enqueue( counters );
			Log.Debug( "TelegrafObserver received  notification." );
		}
	}

	public static class SetupStatistics
	{
		public static void Init( string environemnt, string sysName, string id )
		{
			ConfigureStatsD( environemnt, sysName, id );
		}

		static void ConfigureStatsD( string environemnt, string sysName, string id )
		{
			var env = environemnt.ToLowerInvariant();
			var systemName = sysName.Replace( ".", "-" );

			var node = id.Replace( ".", "-" ).Replace( "SkuVault-", "" );
			var metricsConfig = new MetricsConfig
			{
				Tags = new Dictionary< string, string >()
				{
					{ "env", env },
					{ "system", systemName },
					{ "node", node },
				}
			};

			Metrics.Configure( metricsConfig );
		}
	}
}