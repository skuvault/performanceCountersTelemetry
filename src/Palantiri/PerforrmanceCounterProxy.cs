using System.Diagnostics;

namespace Palantiri
{
	public class PerforrmanceCounterProxy
	{
		public PerforrmanceCounterProxy( PerformanceCounter counter, string @alias )
		{
			this.Counter = counter;
			this.Alias = alias;
		}

		public PerforrmanceCounterProxy( PerformanceCounter counter ): this( counter, null )
		{
		}

		public PerformanceCounter Counter{ get; private set; }
		public string Alias{ get; private set; }
	}
}