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
using System.Windows.Media;
using System.Windows.Shapes;

namespace Aba.Silverlight.WP8.OsMo.Views
{
	public partial class MapView : UserControl
	{
		private Geolocator MapLocator { get; set; }
		private string LockedUser { get; set; }

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
			App.ViewModel.GroupsModel.PropertyChanged += GroupsModel_PropertyChanged;
		}

		void GroupsModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Tracks":
					SetUsers();
					break;
			}
		}

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "Coordinate":
					SetUsers();
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
			SetUsers();
		}

		void MapControl_CenterChanged(object sender, MapCenterChangedEventArgs e)
		{
			App.ViewModel.SettingsModel.MapCenter = MapControl.Center;
			SetUsers();
		}

		public void SetUsers()
		{
			if (!App.ViewModel.IsMapVisible) return;

			// clean up
			if (MapControl.Layers.Count == 0) MapControl.Layers.Add(new MapLayer());
			var layer = MapControl.Layers.First();
			layer.Clear();

			//set me
			var me = App.ViewModel.Coordinate;
			if (me != null)
			{
				var myColor = ConvertHtmlColor("#123123");
				layer.Add(new MapOverlay
				{
					GeoCoordinate = new GeoCoordinate(me.Lat, me.Lon),
					Content = new GeoUser
					{
						Name = "Me",
						Speed = me.SpeedKmh.ToString("N0"),
						Fill = new SolidColorBrush(myColor),
						Stroke = new SolidColorBrush(MakeDarker(myColor)),
						U = "-1",
					},
					ContentTemplate = this.Resources["MeTemplate"] as DataTemplate,
				});
			}

			// set others
			if (App.ViewModel.GroupsModel.Tracks != null)
			{
				foreach (var user in App.ViewModel.GroupsModel.Tracks)
				{
					var groupUser = App.ViewModel.GroupsModel.Groups.Where(w => w.Active).SelectMany(s => s.Users).FirstOrDefault(f => f.U == user.Key);
					if (groupUser == null) continue;
					var coordinate = user.Value[user.Value.Count - 1];
					var geoCoordinate = new GeoCoordinate(coordinate.Lat, coordinate.Lon);
					var point = MapControl.ConvertGeoCoordinateToViewportPoint(geoCoordinate);
					if (point.X < -100 || point.X > MapControl.RenderSize.Width + 100 || point.Y < -100 || point.Y > MapControl.RenderSize.Height + 100) continue;
					var color = ConvertHtmlColor(groupUser.Color);
					layer.Add(new MapOverlay
					{
						GeoCoordinate = geoCoordinate,
						Content = new GeoUser
						{
							Name = groupUser.Name,
							Speed = coordinate.SpeedKmh.ToString("N0"),
							Fill = new SolidColorBrush(color),
							Stroke = new SolidColorBrush(MakeDarker(color)),
							U = user.Key,
						},
						ContentTemplate = this.Resources["UserTemplate"] as DataTemplate
					});
				};
			}

			if (LockedUser != null)
			{
				var user = layer.FirstOrDefault(f => (f.Content as GeoUser).U == LockedUser);
				if (user != null)
				{
					var xcenter = MapControl.RenderSize.Width / 2;
					var ycenter = MapControl.RenderSize.Height / 2;
					var point = MapControl.ConvertGeoCoordinateToViewportPoint(user.GeoCoordinate);
					var distance = Math.Sqrt(Math.Pow(xcenter - point.X, 2) + Math.Pow(ycenter - point.Y, 2));
					if(distance > 50)
					{
						MapControl.Center = user.GeoCoordinate;
					}
				}
			}
		}

		private Color MakeDarker(Color color)
		{
			color.R -= ((byte)(color.R / 10));
			color.G -= ((byte)(color.G / 10));
			color.B -= ((byte)(color.B / 10));
			return color;
		}

		private Color ConvertHtmlColor(string hex)
		{
			hex = hex.TrimStart('#');

			byte a = 255;
			byte r = 255;
			byte g = 255;
			byte b = 255;

			int start = 0;

			if (hex.Length == 8)
			{
				a = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
				start = 2;
			}

			r = byte.Parse(hex.Substring(start, 2), System.Globalization.NumberStyles.HexNumber);
			g = byte.Parse(hex.Substring(start + 2, 2), System.Globalization.NumberStyles.HexNumber);
			b = byte.Parse(hex.Substring(start + 4, 2), System.Globalization.NumberStyles.HexNumber);

			return Color.FromArgb(a, r, g, b);
		}

		public class GeoUser
		{
			public string Speed { get; set; }
			public SolidColorBrush Fill { get; set; }
			public SolidColorBrush Stroke { get; set; }
			public string Name { get; set; }
			public string U { get; set; }
		}

		private void UserPushpin_Click(object sender, RoutedEventArgs e)
		{
			var button = sender as Button;
			var user = button.DataContext.ToString();
			if (LockedUser == user)
			{
				LockedUser = null;
			}
			else
			{
				LockedUser = user;
			}
		}
	}
}
