namespace Palantiri.Counters
{
	public class CounterName
	{
		public string Name { get; private set; }

		public CounterName(string name)
		{
			this.Name = name;
		}
	}
}