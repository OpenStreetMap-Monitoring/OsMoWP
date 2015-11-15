using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Aba.Silverlight.WP8.OsMo.Resources;
using Windows.Devices.Geolocation;
using Windows.Phone.System.Analytics;
using System.Collections.Generic;

namespace Aba.Silverlight.WP8.OsMo.ViewModels
{
	public class MainViewModel : BaseModel
	{
		protected override string[] _ModelNotifyFields { get { return new string[] { "IsServiceStarted", "Coordinate", "MessageOfTheDay" }; } }

		private GroupsModel _GroupsModel;
		public GroupsModel GroupsModel { get { if (_GroupsModel == null) _GroupsModel = new GroupsModel(); return _GroupsModel; } }

		private SettingsModel _SettingsModel;
		public SettingsModel SettingsModel { get { if (_SettingsModel == null) _SettingsModel = new SettingsModel(); return _SettingsModel; } }

		private bool _IsServiceStarted = false;
		public bool IsServiceStarted { get { return _IsServiceStarted; } set { if (value != _IsServiceStarted) { _IsServiceStarted = value; NotifyPropertyChanged(); } } }

		private Geocoordinate _Coordinate;
		public Geocoordinate Coordinate { get { return _Coordinate; } set { if (value != _Coordinate) { _Coordinate = value; NotifyPropertyChanged(); } } }

		private string _MessageOfTheDay;
		public string MessageOfTheDay { get { return _MessageOfTheDay; } set { if (value != _MessageOfTheDay) { _MessageOfTheDay = value; NotifyPropertyChanged(); } } }

		private ObservableCollection<string> _DebugLog;
		public ObservableCollection<string> DebugLog { get { if (_DebugLog == null) _DebugLog = new ObservableCollection<string>(); return _DebugLog; } set { if (value != _DebugLog) { _DebugLog = value; NotifyPropertyChanged(); } } }

		public string TrackerId { get { return HostInformation.PublisherHostId; } }

		public void AddDebugLog(string line)
		{
			if (SettingsModel.DebugViewEnabled.Value)
			{
				App.RootFrame.Dispatcher.BeginInvoke(() => { DebugLog.Add(line.Trim()); });
			}
		}
	}
}