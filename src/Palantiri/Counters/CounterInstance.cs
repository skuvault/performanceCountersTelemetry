namespace Palantiri.Counters
{
	public class CounterInstance

	{
		public string Instance { get; private set; }

		public CounterInstance(string instance)
		{
			this.Instance = instance;
		}
	}
}