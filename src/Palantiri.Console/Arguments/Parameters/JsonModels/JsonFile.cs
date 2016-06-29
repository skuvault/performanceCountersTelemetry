using System.Collections.Generic;

namespace Palantiri.Console.Arguments.Parameters.JsonModels
{
	public class JsonConfig
	{
		public List< Counter > Counters{ get; set; }
		public List< Destination > Destinations{ get; set; }
		public int Period{ get; set; }
	}
}