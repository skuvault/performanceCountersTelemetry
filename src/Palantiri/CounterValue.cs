using System;

namespace Palantiri
{
	public class CounterValue
	{
		public DateTime DateTime{ get; private set; }
		public float Value{ get; private set; }

		public CounterValue( DateTime dateTime, float value )
		{
			this.DateTime = dateTime;
			this.Value = value;
		}
	}
}