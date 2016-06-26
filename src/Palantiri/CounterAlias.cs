namespace Palantiri
{
	public class CounterAlias
	{
		public string Alias{ get; private set; }

		public CounterAlias( string alias )
		{
			this.Alias = alias;
		}
	}
}