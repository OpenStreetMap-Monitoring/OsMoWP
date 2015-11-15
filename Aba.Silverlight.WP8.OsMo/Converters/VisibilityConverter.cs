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
	public class VisibilityConverter : IValueConverter
	{

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null) return GetCollapsed(parameter);
			if (value is bool) return (bool)value ? GetVisible(parameter) : GetCollapsed(parameter);
			if (value is bool?) return ((bool?)value).GetValueOrDefault() ? GetVisible(parameter) : GetCollapsed(parameter);
			if (value is ICollection) return (value as ICollection).Count > 0 ? GetVisible(parameter) : GetCollapsed(parameter);
			if (value is Visibility) return ((Visibility)value == Visibility.Visible) ? GetVisible(parameter) : GetCollapsed(parameter);
			if (value is string) return string.IsNullOrEmpty(value as string) ? GetCollapsed(parameter) : GetVisible(parameter);
			return GetVisible(parameter);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		private Visibility GetVisible(object parameter)
		{
			if (parameter == null) return Visibility.Visible; else return Visibility.Collapsed;
		}

		private Visibility GetCollapsed(object parameter)
		{
			if (parameter == null) return Visibility.Collapsed; else return Visibility.Visible;
		}
	}

}
