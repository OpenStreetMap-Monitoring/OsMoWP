using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Windows.Devices.Geolocation;

namespace Aba.Silverlight.WP8.OsMo.Converters
{
	public class CoordinateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var coordinate = value as Geocoordinate;
			if (coordinate == null) return null;
			return string.Format("{0}, {1}", coordinate.Latitude, coordinate.Longitude );
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
