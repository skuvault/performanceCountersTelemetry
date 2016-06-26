using System;
using System.Collections.Generic;
using System.Linq;
using Palantiri.Sensors;

namespace Palantiri
{
	public class SensorManager
	{
		protected static readonly List< Sensor > Sensors = new List< Sensor >();
		protected static SensorManager _sensorManager;

		public static SensorManager GetSingleton()
		{
			return _sensorManager ?? ( _sensorManager = new SensorManager() );
		}

		protected SensorManager()
		{
		}

		public void AddSensors( Sensor[] sensors )
		{
			if( sensors.Length + Sensors.Count > 1 )
				throw new Exception( "You can't create more the 1 sensor" );
			Sensors.AddRange( sensors );
		}

		public int GetSensorsCount()
		{
			return Sensors.Count;
		}

		public Sensor GetSensorTask()
		{
			var task = Sensors.FirstOrDefault();
			if( task == null )
				throw new Exception( "There is no sensor" );
			return task;
		}
	}
}