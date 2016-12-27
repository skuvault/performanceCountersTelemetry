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
}