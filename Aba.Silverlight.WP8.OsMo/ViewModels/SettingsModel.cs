using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Aba.Silverlight.WP8.OsMo.Resources;
using Windows.Devices.Geolocation;
using Windows.Phone.System.Analytics;
using System.Collections.Generic;
using Aba.Silverlight.WP8.OsMo.Models;
using System.IO.IsolatedStorage;
using Windows.Foundation.Metadata;
using System.Runtime.CompilerServices;
using System.Device.Location;

namespace Aba.Silverlight.WP8.OsMo.ViewModels
{
	public class SettingsModel : BaseModel
	{
		protected override string[] _ModelNotifyFields { get { return new string[] { }; } }

		private uint? _ReportInterval = (uint?)SettingsModel.Get();
		public uint? ReportInterval
		{
			get { return _ReportInterval; }
			set { if (value != _ReportInterval) { if (value != null && value >= 50 && value <= 60000) { _ReportInterval = value; Save(value); } NotifyPropertyChanged(); } }
		}

		private double? _MovementThreshold = (double?)SettingsModel.Get();
		public double? MovementThreshold
		{
			get { return _MovementThreshold; }
			set { if (value != _MovementThreshold) { if (value != null && value >= 0 && value <= 100) { _MovementThreshold = value; Save(value); } NotifyPropertyChanged(); } }
		}

		private uint? _DesiredAccuracyInMeters = (uint?)SettingsModel.Get();
		public uint? DesiredAccuracyInMeters
		{
			get { return _DesiredAccuracyInMeters; }
			set { if (value != _DesiredAccuracyInMeters) { if (value != null && value >= 10 && value <= 100) { _DesiredAccuracyInMeters = value; Save(value); } NotifyPropertyChanged(); } }
		}

		private bool? _DebugViewEnabled = (bool?)SettingsModel.Get();
		public bool? DebugViewEnabled
		{
			get { return _DebugViewEnabled; }
			set { if (value != _DebugViewEnabled) { _DebugViewEnabled = value; Save(value); NotifyPropertyChanged(); } }
		}

		private bool? _PersistentConnection = (bool?)SettingsModel.Get();
		public bool? PersistentConnection
		{
			get { return _PersistentConnection; }
			set { if (value != _PersistentConnection) { _PersistentConnection = value; Save(value); NotifyPropertyChanged(); } }
		}

		private GeoCoordinate _MapCenter = (GeoCoordinate)SettingsModel.Get();
		public GeoCoordinate MapCenter
		{
			get { return _MapCenter; }
			set { if (value != _MapCenter) { _MapCenter = value; Save(value); NotifyPropertyChanged(); } }
		}

		private double? _MapZoom = (double?)SettingsModel.Get();
		public double? MapZoom
		{
			get { return _MapZoom; }
			set { if (value != _MapZoom) { _MapZoom = value; Save(value); NotifyPropertyChanged(); } }
		}

		private void Save(object value, [CallerMemberName] string propertyName = "none passed")
		{
			lock (IsolatedStorageSettings.ApplicationSettings)
			{
				IsolatedStorageSettings.ApplicationSettings[propertyName] = value;
				IsolatedStorageSettings.ApplicationSettings.Save();
			}
		}

		private static object Get([CallerMemberName] string propertyName = "none passed")
		{
			var name = propertyName.TrimStart('_');
			return IsolatedStorageSettings.ApplicationSettings.Contains(name) ? IsolatedStorageSettings.ApplicationSettings[name] : null;
		}

		public SettingsModel()
		{
			if (ReportInterval == null) ReportInterval = 1000;
			if (MovementThreshold == null) MovementThreshold = 1;
			if (DesiredAccuracyInMeters == null) DesiredAccuracyInMeters = 10;
			if (DebugViewEnabled == null) DebugViewEnabled = false;
			if (PersistentConnection == null) PersistentConnection = false;
			if (MapCenter == null) MapCenter = new GeoCoordinate();
			if (MapZoom == null) MapZoom = 2;
		}
	}
}