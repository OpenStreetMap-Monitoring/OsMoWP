using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Aba.Silverlight.WP8.OsMo.Resources;
using Windows.Devices.Geolocation;
using Windows.Phone.System.Analytics;

namespace Aba.Silverlight.WP8.OsMo.ViewModels
{
	public class MainViewModel : BaseModel
	{
		protected override string[] _ModelNotifyFields { get { return new string[] { }; } }

		public static Geolocator Geolocator { get; set; }
		public static bool RunningInBackground { get; set; }

		private bool _IsServiceStarted = false;
		public bool IsServiceStarted { get { return _IsServiceStarted; } set { if (value != _IsServiceStarted) { _IsServiceStarted = value; NotifyPropertyChanged(); } } }

		private Geocoordinate _Coordinate;
		public Geocoordinate Coordinate { get { return _Coordinate; } set { if (value != _Coordinate) { _Coordinate = value; NotifyPropertyChanged(); } } }

		public string DeviceId { get { return HostInformation.PublisherHostId; } }

		public MainViewModel()
		{
		}

	}
}