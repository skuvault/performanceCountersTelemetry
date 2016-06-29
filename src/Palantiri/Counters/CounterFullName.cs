namespace Palantiri.Counters
{
	public class CounterFullName
	{
		public CounterName Name{ get; private set; }
		public CounterCategory Category { get; private set; }
		public CounterInstance Instance { get; private set; }

		public CounterFullName(CounterName name, CounterCategory category, CounterInstance instance )
		{
			this.Name = name;
			this.Category = category;
			this.Instance = instance;
		}
	}
}