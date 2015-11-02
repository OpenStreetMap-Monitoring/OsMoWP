using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Aba.Silverlight.WP8.OsMo.Converters
{
	public class BoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null) return GetFalse(parameter);
			if (value is bool) return (bool)value ? GetTrue(parameter) : GetFalse(parameter);
			if (value is ICollection) return (value as ICollection).Count > 0 ? GetTrue(parameter) : GetFalse(parameter);
			if (value is Visibility) return ((Visibility)value == Visibility.Visible) ? GetTrue(parameter) : GetFalse(parameter);
			return GetTrue(parameter);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		private bool GetTrue(object parameter)
		{
			if(parameter == null) return false; else return true;
		}

		private bool GetFalse(object parameter)
		{
			if(parameter == null) return true; else return false;

		}
	}
}
