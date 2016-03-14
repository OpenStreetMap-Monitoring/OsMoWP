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
		private int _LoginTabPosition;
		private ApplicationBarMenuItem DebugMenuItem { get; set; }

		public MainPage()
		{
			(App.Current as App).Page = this;
			InitializeComponent();
			DataContext = App.ViewModel;

			ApplicationBar = new ApplicationBar();

			ApplicationBar.Buttons.Add(new ApplicationBarIconButton
			{
				IconUri = new Uri("/Assets/AppBar/appbar.tracking.png", UriKind.Relative),
				Text = AppResources.TrackingTabTitle
			});

			ApplicationBar.Buttons.Add(new ApplicationBarIconButton
			{
				IconUri = new Uri("/Assets/AppBar/appbar.map.png", UriKind.Relative),
				Text = AppResources.MapTabTitle
			});

			ApplicationBar.Buttons.Add(new ApplicationBarIconButton
			{
				IconUri = new Uri("/Assets/AppBar/appbar.group.png", UriKind.Relative),
				Text = AppResources.GroupsTabTitle
			});

			ApplicationBar.Buttons.Add(new ApplicationBarIconButton
			{
				IconUri = new Uri("/Assets/AppBar/appbar.settings.png", UriKind.Relative),
				Text = AppResources.SettingsTabTitle
			});

			foreach (ApplicationBarIconButton b in ApplicationBar.Buttons) b.Click += AppBarButton_Click;

			ApplicationBar.MenuItems.Add(new ApplicationBarMenuItem(AppResources.TrackingTabTitle));
			ApplicationBar.MenuItems.Add(new ApplicationBarMenuItem(AppResources.MapTabTitle));
			ApplicationBar.MenuItems.Add(new ApplicationBarMenuItem(AppResources.GroupsTabTitle));
			ApplicationBar.MenuItems.Add(new ApplicationBarMenuItem(AppResources.LoginTabTitle));
			ApplicationBar.MenuItems.Add(new ApplicationBarMenuItem(AppResources.SettingsTabTitle));
			foreach (ApplicationBarMenuItem i in ApplicationBar.MenuItems) i.Click += AppBarMenuItem_Click;

			DebugMenuItem = new ApplicationBarMenuItem(AppResources.DebugTabTitle);
			DebugMenuItem.Click += AppBarMenuItem_Click;

			if(App.ViewModel.SettingsModel.DebugViewEnabled.Value)
			{
				ApplicationBar.MenuItems.Add(DebugMenuItem);
			}
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
#if DEBUG
			App.ViewModel.SettingsModel.DebugViewEnabled = true;
#endif
			CheckLoginTab();
		}

		public void CheckLoginTab()
		{
			var iss = IsolatedStorageSettings.ApplicationSettings;
			if (iss.Contains(Messenger.SERVER_DEVICE_ID) && !iss.Contains(Messenger.SERVER_USER_KEY))
			{
				App.ViewModel.IsLoginVisible = true;
				LoginBrowser.Source = new Uri(string.Format("https://osmo.mobi/signin?type=m&key={0}", iss[Messenger.SERVER_DEVICE_ID]));
			}
			else
			{
				App.ViewModel.IsLoginVisible = false;
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
				ApplicationBar.MenuItems.Add(DebugMenuItem);
			}
			else if (!App.ViewModel.SettingsModel.DebugViewEnabled.Value && Pivot.Items.Contains(DebugTab))
			{
				Pivot.Items.Remove(DebugTab);
				ApplicationBar.MenuItems.Remove(DebugMenuItem);
			}
		}

		private void SwitchTab(string title)
		{
			var switchTo = Pivot.Items.OfType<PivotItem>().FirstOrDefault(f => f.Header.ToString() == title);
			if (switchTo != null) Pivot.SelectedItem = switchTo;
		}

		private void AppBarButton_Click(object sender, EventArgs e)
		{
			var button = sender as ApplicationBarIconButton;
			SwitchTab(button.Text);
		}

		private void AppBarMenuItem_Click(object sender, EventArgs e)
		{
			var mi = sender as ApplicationBarMenuItem;
			SwitchTab(mi.Text);
		}

		private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

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
			var button = sender as Button;
			button.IsEnabled = false;
			var body = new StringBuilder();
			body.AppendFormat("{0}\r\n", System.Reflection.Assembly.GetExecutingAssembly().FullName);
			var iss = IsolatedStorageFile.GetUserStoreForApplication();
			var crashList = App.ViewModel.CrashReports;
			foreach (var name in crashList)
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
			var client = new WebClient();
			client.UploadStringCompleted += (se, ev) =>
			{
				if (ev.Result == "1")
				{
					foreach (var file in ev.UserState as List<string>)
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
				}
				button.IsEnabled = true;
				App.ViewModel.CrashReports = null;
			};
			client.UploadStringAsync(new Uri("http://seventhside.org/Debug/Upload"), "POST", body.ToString(), crashList);
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
				}
				App.ViewModel.IsLoginVisible = false;
				App.Messenger.Disconnect();
				App.Messenger.CGroup();
			}
		}

		private void LogoffButton_Click(object sender, RoutedEventArgs e)
		{
			IsolatedStorageSettings.ApplicationSettings.Remove(Messenger.SERVER_USER_KEY);
			IsolatedStorageSettings.ApplicationSettings.Save();
			App.Messenger.Disconnect();
			App.Messenger.CGroup();
			CheckLoginTab();
		}

	}
}