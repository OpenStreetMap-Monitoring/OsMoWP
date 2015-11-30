using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace Aba.Silverlight.WP8.OsMo.Models
{
	public class Coordinate
	{
		public double Lat { get; set; }
		public double Lon { get; set; }
		public int HDOP { get; set; }
		public int Alt { get; set; }
		public double Speed { get; set; }
		public int SpeedKmh { get { return Convert.ToInt32(Speed * 3.6); } }
		public int Course { get; set; }
		public int Timestamp { get; set; }
		public static Coordinate Create(Geocoordinate underlayed)
		{
			return new Coordinate
			{
				Alt = Convert.ToInt32(underlayed.Altitude),
				HDOP = Convert.ToInt32(underlayed.Accuracy),
				Course = Convert.ToInt32(underlayed.Heading),
				Lat = underlayed.Latitude,
				Lon = underlayed.Longitude,
				Speed = underlayed.Speed.GetValueOrDefault(0),
				Timestamp = Convert.ToInt32(underlayed.Timestamp.UtcDateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds),
			};
		}
	}
}
