using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Palantir.Properties;
using Telegraf;

namespace Palantir.SensorObservers
{
	public class TelegrafObserver: ISensorObserver
	{
		protected Task _consoleWriter;
		protected readonly int _period;
		protected readonly int _maxInstancesToProcess;
		protected readonly CancellationTokenSource cts;
		protected readonly CancellationToken ct;
		protected ConcurrentQueue< ConcurrentDictionary< string, float > > _buffer;

		public TelegrafObserver()
		{
			SetupStatistics.Init( Settings.Default.TelegrafEnv, Settings.Default.TelegrafSysName, Settings.Default.TelegrafId );
			this._buffer = new ConcurrentQueue< ConcurrentDictionary< string, float > >();
			this._period = 500;
			this.cts = new CancellationTokenSource();
			this.ct = this.cts.Token;
			this._maxInstancesToProcess = 10;
			this.StartListening();
		}

		protected void StartListening()
		{
			this._consoleWriter = Task.Factory.StartNew( () =>
			{
				while( !this.ct.IsCancellationRequested )
				{
					this.Listen();
					Task.Delay( this._period ).Wait( this.ct );
				}
			}
				);
		}

		protected void Listen()
		{
			if( this._buffer != null )
			{
				ConcurrentDictionary< string, float > res;
				for( var i = 0; i < this._maxInstancesToProcess && this._buffer.TryDequeue( out res ); i++ )
				{
					var values = res.ToDictionary( x => x.Key, y => ( object )y.Value );
					Metrics.Record( "app-sys-counters", values );
				}
			}
		}

		~TelegrafObserver()
		{
			if( this._consoleWriter != null && this.cts != null && this.cts.IsCancellationRequested )
				this.cts.Cancel();
		}

		public void SendCounters( ConcurrentDictionary< string, float > counters )
		{
			this._buffer.Enqueue( counters );
		}
	}

	public static class SetupStatistics
	{
		public const string StatisticsServer = "127.0.0.1";
		public const int StatisticsPort = 8125;

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