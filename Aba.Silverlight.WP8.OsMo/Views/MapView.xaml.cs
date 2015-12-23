using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Maps.Controls;
using System.Device.Location;
using Windows.Devices.Geolocation;

namespace Aba.Silverlight.WP8.OsMo.Views
{
	public partial class MapView : UserControl
	{
		private Geolocator MapLocator { get; set; }

		public MapView()
		{
			InitializeComponent();
			Loaded += MapView_Loaded;
		}

		void MapView_Loaded(object sender, RoutedEventArgs e)
		{
			Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId = "99d80c35-95b0-4a60-b584-03bdc480abf9";
			Microsoft.Phone.Maps.MapsSettings.ApplicationContext.AuthenticationToken = "pZWUsZa55FOvRLSasWgUQQ";
#if DEBUG
			MapControl.ZoomLevel = 2;
#endif
			MapControl.Center = App.ViewModel.SettingsModel.MapCenter;
			MapControl.TileSources.Add(new TileSource("http://a.tile.openstreetmap.org/{zoomLevel}/{x}/{y}.png"));
			MapControl.CenterChanged += MapControl_CenterChanged;
			MapControl.ZoomLevelChanged += MapControl_ZoomLevelChanged;
			App.ViewModel.PropertyChanged += ViewModel_PropertyChanged;
		}

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Coordinate":
					SetMe(App.ViewModel.Coordinate);
					break;
				case "IsMapVisible":
					if (App.ViewModel.IsMapVisible)
					{
						MapLocator = new Geolocator
						{
							DesiredAccuracy = PositionAccuracy.High,
							DesiredAccuracyInMeters = App.ViewModel.SettingsModel.DesiredAccuracyInMeters,
							MovementThreshold = App.ViewModel.SettingsModel.MovementThreshold.Value,
							ReportInterval = App.ViewModel.SettingsModel.ReportInterval.Value,
						};
						MapLocator.PositionChanged += MapLocator_PositionChanged;
					}
					else
					{
						MapLocator.PositionChanged -= MapLocator_PositionChanged;
						MapLocator = null;
					}
					break;
			}
		}

		void MapLocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
		{
			Dispatcher.BeginInvoke(() => { App.ViewModel.Coordinate = Models.Coordinate.Create(args.Position.Coordinate); });
		}

		void MapControl_ZoomLevelChanged(object sender, MapZoomLevelChangedEventArgs e)
		{
			App.ViewModel.SettingsModel.MapZoom = MapControl.ZoomLevel;
		}

		void MapControl_CenterChanged(object sender, MapCenterChangedEventArgs e)
		{
			App.ViewModel.SettingsModel.MapCenter = MapControl.Center;
		}

		public void SetMe(Models.Coordinate me)
		{
			if (MapControl.Layers.Count == 0) MapControl.Layers.Add(new MapLayer());
			var layer = MapControl.Layers.First();
			layer.Clear();
			layer.Add(new MapOverlay
			{
				GeoCoordinate = new GeoCoordinate(me.Lat, me.Lon),
				Content = me.SpeedKmh.ToString("N0"),
				ContentTemplate = this.Resources["MeTemplate"] as DataTemplate
			});
		}
	}
}
