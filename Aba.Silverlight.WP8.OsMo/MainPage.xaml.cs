using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Aba.Silverlight.WP8.OsMo.Resources;
using Microsoft.Phone.Scheduler;
using Aba.Silverlight.WP8.OsMo.ViewModels;
using Windows.Devices.Geolocation;
using Windows.Phone.System.Analytics;
using Microsoft.Phone.Tasks;
using System.Text;
using System.IO.IsolatedStorage;
using System.IO;
using System.Text.RegularExpressions;

namespace Aba.Silverlight.WP8.OsMo
{
	public partial class MainPage : PhoneApplicationPage
	{

		public MainPage()
		{
			InitializeComponent();
			DataContext = App.ViewModel;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			(App.Current as App).Page = this;
			var iss = IsolatedStorageSettings.ApplicationSettings;
			if (iss.Contains(Messenger.SERVER_DEVICE_ID) && !iss.Contains(Messenger.SERVER_USER_KEY))
			{
				LoginBrowser.Source = new Uri(string.Format("https://osmo.mobi/signin?type=m&key={0}", IsolatedStorageSettings.ApplicationSettings[Messenger.SERVER_DEVICE_ID]));
			}
			else
			{
				Pivot.Items.Remove(LoginTab);
			}
		}

		private void ServiceButton_Click(object sender, RoutedEventArgs e)
		{
			App.ViewModel.IsServiceStarted = !App.ViewModel.IsServiceStarted;
			if (App.ViewModel.IsServiceStarted)
			{
				(App.Current as App).StartTracking();
			}
			else
			{
				(App.Current as App).StopTracking();
			}
		}

		public void CheckDebugTab()
		{
			if (App.ViewModel.SettingsModel.DebugViewEnabled.Value && !Pivot.Items.Contains(DebugTab))
			{
				Pivot.Items.Add(DebugTab);
			}
			else if (!App.ViewModel.SettingsModel.DebugViewEnabled.Value && Pivot.Items.Contains(DebugTab))
			{
				Pivot.Items.Remove(DebugTab);
			}
		}

		private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (Pivot.SelectedItem == GroupsTab && App.ViewModel.GroupsModel.Groups == null)
			{
				App.Messenger.CGroup();
			}

			if (Pivot.SelectedItem == MapTab)
			{
				App.ViewModel.IsMapVisible = true;
			}
			else
			{
				App.ViewModel.IsMapVisible = false;
			}
			this.Dispatcher.BeginInvoke(CheckDebugTab);
		}

		private void SessionButton_Click(object sender, RoutedEventArgs e)
		{
			var webTask = new WebBrowserTask { Uri = new Uri(string.Format("https://osmo.mobi/s/{0}", App.ViewModel.SessionId)) };
			webTask.Show();
		}

		private void SendReportsButton_Click(object sender, RoutedEventArgs e)
		{
			var body = new StringBuilder();
			body.AppendFormat("{0}\r\n", System.Reflection.Assembly.GetExecutingAssembly().FullName);
			var iss = IsolatedStorageFile.GetUserStoreForApplication();
			foreach (var name in App.ViewModel.CrashReports)
			{
				try
				{
					using (var file = iss.OpenFile(name, System.IO.FileMode.Open))
					{
						var reader = new StreamReader(file);
						body.AppendFormat("\r\n\r\nFileDate:{0:s}\r\n", iss.GetCreationTime(name).UtcDateTime);
						body.Append(reader.ReadToEnd());
					}
				}
				catch (Exception ex)
				{
					(App.Current as App).Crash(ex);
				}
			}
			var email = new EmailComposeTask { To = "abaland@gmail.com", Bcc = "support@osmo.mobi", Subject = "WP8 crash reports", Body = body.ToString() };
			email.Show();
			foreach (var file in App.ViewModel.CrashReports)
			{
				try
				{
					iss.DeleteFile(file);
				}
				catch (Exception ex)
				{
					(App.Current as App).Crash(ex);
				}
			}
			App.ViewModel.CrashReports = null;
		}

		private void SaveLogButton_Click(object sender, RoutedEventArgs e)
		{
			(App.Current as App).Crash(string.Join("\r\n", App.ViewModel.DebugLog));
			App.ViewModel.CrashReports = null;
		}

		private void LoginBrowser_Navigated(object sender, NavigationEventArgs e)
		{
			var match = Regex.Match(e.Uri.ToString(), "^https[:][/][/]api[.]osmo[.]mobi[/]rd[?]nick=([a-z0-9]+)&user=([a-z0-9]+)&", RegexOptions.IgnoreCase);
			if (match.Success)
			{
				lock (IsolatedStorageSettings.ApplicationSettings)
				{
					IsolatedStorageSettings.ApplicationSettings[Messenger.SERVER_USER_KEY] = match.Groups[2].Value;
					IsolatedStorageSettings.ApplicationSettings.Save();
					Pivot.Items.Remove(LoginTab);
				}
			}
		}

	}
}