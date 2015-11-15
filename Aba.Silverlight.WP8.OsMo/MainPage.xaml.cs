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
			if (Pivot.SelectedIndex == 1 && App.ViewModel.GroupsModel.Groups == null)
			{
				App.Messenger.CGroup();
			}
			CheckDebugTab();
		}
	}
}