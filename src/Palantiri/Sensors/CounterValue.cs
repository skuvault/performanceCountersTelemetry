using System;

namespace Palantiri.Sensors
{
	public class CounterValue
	{
		public DateTime _dateTime{ get; private set; }
		public float _value{ get; private set; }

		public CounterValue( DateTime dateTime, float value )
		{
			this._dateTime = dateTime;
			this._value = value;
		}
	}
}