﻿using System;
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
		}

		private void ServiceButton_Click(object sender, RoutedEventArgs e)
		{
			App.ViewModel.IsServiceStarted = !App.ViewModel.IsServiceStarted;
			if (App.ViewModel.IsServiceStarted)
			{
				MainViewModel.Geolocator = new Geolocator
				{
					DesiredAccuracy = PositionAccuracy.High,
					DesiredAccuracyInMeters = 5,
					MovementThreshold = 5,
					ReportInterval = 1000
				};

				MainViewModel.Geolocator.PositionChanged += Geolocator_PositionChanged;
			}
			else
			{
				MainViewModel.Geolocator.PositionChanged -= Geolocator_PositionChanged;
				MainViewModel.Geolocator = null;
			}
		}

		void Geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
		{
			if (MainViewModel.RunningInBackground)
			{

			}
			else
			{
				Dispatcher.BeginInvoke(() =>
				{
					App.ViewModel.Coordinate = args.Position.Coordinate;
				});
			}
		}
	}
}