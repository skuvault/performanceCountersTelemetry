namespace Palantiri.Counters
{
	public class CounterAlias
	{
		public string Alias{ get; private set; }

		public CounterAlias( string alias )
		{
			this.Alias = alias;
		}

		public bool IsEmpty()
		{
			return string.IsNullOrWhiteSpace( Alias );
		}
	}

	public class CounterParameters
	{
		public bool DevideByCpuCoresCount { get; private set; }

		public CounterParameters(bool devideByCpuCoresCount)
		{
			this.DevideByCpuCoresCount = devideByCpuCoresCount;
		}
	}
}