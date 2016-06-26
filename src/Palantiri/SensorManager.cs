using System.Collections.Generic;
using Palantiri.Sensors;

namespace Palantiri
{
	public class SensorManager
	{
		protected static readonly List< Sensor > Sensors = new List< Sensor >();
		protected static SensorManager _sensorManager;

		public static SensorManager GetSensorManager()
		{
			return _sensorManager ?? ( _sensorManager = new SensorManager() );
		}

		protected SensorManager()
		{
		}
	}
}