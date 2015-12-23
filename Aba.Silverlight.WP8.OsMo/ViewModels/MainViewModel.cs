using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Aba.Silverlight.WP8.OsMo.Resources;
using Aba.Silverlight.WP8.OsMo.Models;
using Windows.Devices.Geolocation;
using Windows.Phone.System.Analytics;
using System.Collections.Generic;
using System.IO.IsolatedStorage;

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

		private bool _IsMapVisible = false;
		public bool IsMapVisible { get { return _IsMapVisible; } set { if (value != _IsMapVisible) { _IsMapVisible = value; NotifyPropertyChanged(); } } }

		private Coordinate _Coordinate;
		public Coordinate Coordinate { get { return _Coordinate; } set { if (value != _Coordinate) { _Coordinate = value; NotifyPropertyChanged(); } } }

		private string _MessageOfTheDay;
		public string MessageOfTheDay { get { return _MessageOfTheDay; } set { if (value != _MessageOfTheDay) { _MessageOfTheDay = value; NotifyPropertyChanged(); } } }

		private ObservableCollection<string> _DebugLog;
		public ObservableCollection<string> DebugLog { get { if (_DebugLog == null) _DebugLog = new ObservableCollection<string>(); return _DebugLog; } set { if (value != _DebugLog) { _DebugLog = value; NotifyPropertyChanged(); } } }

		private string _TrackerId;
		public string TrackerId { get { return _TrackerId; } set { if (value != _TrackerId) { _TrackerId = value; NotifyPropertyChanged(); } } }

		private string _SessionId;
		public string SessionId { get { return _SessionId; } set { if (value != _SessionId) { _SessionId = value; NotifyPropertyChanged(); } } }

		private Messenger.ConnectionStatus _MessengerStatus;
		public Messenger.ConnectionStatus MessengerStatus { get { return _MessengerStatus; } set { if (value != _MessengerStatus) { _MessengerStatus = value; NotifyPropertyChanged(); } } }

		public List<string> CrashReports
		{
			get
			{
				try
				{
					return IsolatedStorageFile.GetUserStoreForApplication().GetFileNames("crash-*").ToList();
				}
				catch (Exception ex) { return null; }
			}
			set
			{
				NotifyPropertyChanged();
			}
		}

		public void AddDebugLog(string line)
		{
			if (SettingsModel.DebugViewEnabled.Value)
			{
				App.RootFrame.Dispatcher.BeginInvoke(() => { DebugLog.Add(line.Trim()); });
			}
		}
	}
}