using System;
using System.Linq;
using System.Diagnostics;
using System.Resources;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Aba.Silverlight.WP8.OsMo.Resources;
using Aba.Silverlight.WP8.OsMo.ViewModels;
using Windows.Devices.Geolocation;
using System.IO.IsolatedStorage;
using System.IO;
using System.Text;

namespace Aba.Silverlight.WP8.OsMo
{
	public partial class App : Application
	{

		private static MainViewModel _ViewModel = null;
		public static MainViewModel ViewModel { get { if (_ViewModel == null) { _ViewModel = new MainViewModel(); } return _ViewModel; } }

		private static Messenger _Messenger;
		public static Messenger Messenger { get { if (_Messenger == null) _Messenger = new Messenger(); return _Messenger; } }

		public static Geolocator Geolocator { get; set; }
		public static bool RunningInBackground { get; set; }

		public static PhoneApplicationFrame RootFrame { get; private set; }
		public MainPage Page { get; set; }

		public App()
		{
			UnhandledException += Application_UnhandledException;
			InitializeComponent();
			InitializePhoneApplication();
			InitializeLanguage();
			if (Debugger.IsAttached)
			{
				Application.Current.Host.Settings.EnableFrameRateCounter = true;
				PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
			}
		}

		public void StartTracking()
		{
			App.Geolocator = new Geolocator
			{
				DesiredAccuracy = PositionAccuracy.High,
				DesiredAccuracyInMeters = 50,
				MovementThreshold = 5,
				ReportInterval = 1000
			};
			App.Messenger.CTo();
			App.Geolocator.PositionChanged += Geolocator_PositionChanged;
		}

		public void StopTracking()
		{
			App.Geolocator.PositionChanged -= Geolocator_PositionChanged;
			App.Geolocator = null;
			App.Messenger.CTc();
		}

		private void Application_Launching(object sender, LaunchingEventArgs e)
		{
			ViewModel.SettingsModel.PropertyChanged += SettingsModel_PropertyChanged;
			Messenger.Connect();
		}

		void SettingsModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (Geolocator != null)
			{
				switch (e.PropertyName)
				{
					case "ReportInterval":
						Geolocator.ReportInterval = ViewModel.SettingsModel.ReportInterval.Value;
						break;
					case "MovementThreshold":
						Geolocator.MovementThreshold = ViewModel.SettingsModel.MovementThreshold.Value;
						break;
					case "DesiredAccuracyInMeters":
						Geolocator.DesiredAccuracyInMeters = ViewModel.SettingsModel.DesiredAccuracyInMeters.Value;
						break;
				}
			}
			switch (e.PropertyName)
			{
				case "DebugViewEnabled":
					var debugLogEnabled = ViewModel.SettingsModel.DebugViewEnabled;
					Page.CheckDebugTab();
					break;
			}

		}

		private void Application_Activated(object sender, ActivatedEventArgs e)
		{
			Messenger.Connect();
			App.ViewModel.IsServiceStarted = App.Geolocator != null;
			App.RunningInBackground = false;
		}

		private void Application_Deactivated(object sender, DeactivatedEventArgs e)
		{
			Messenger.Disconnect();
		}

		private void Application_Closing(object sender, ClosingEventArgs e)
		{
			Messenger.Disconnect();
		}

		private void Application_RunningInBackground(object sender, RunningInBackgroundEventArgs e)
		{
			App.RunningInBackground = true;
		}

		private void Geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
		{
			App.Messenger.CT(args.Position.Coordinate);
			if (!App.RunningInBackground)
			{
				App.RootFrame.Dispatcher.BeginInvoke(() => { App.ViewModel.Coordinate = args.Position.Coordinate; });
			}
		}



		public void Crash(Exception e)
		{
			var report = new StringBuilder();

			if (e == null)
			{
				report.Append("empty crash");
			}
			report.AppendLine(e.Message);
			report.AppendLine(e.Source);
			report.AppendLine(e.StackTrace);
			if (e.InnerException != null)
			{
				report.AppendLine(e.InnerException.Message);
			}
			Crash(report.ToString());
		}

		public void Crash(string report)
		{
			using (var file = IsolatedStorageFile.GetUserStoreForApplication().CreateFile(string.Format("crash-{0}", Guid.NewGuid())))
			{
				using (var writer = new StreamWriter(file))
				{
					if (string.IsNullOrWhiteSpace(report))
					{
						writer.WriteLine("empty report");
					}
					else
					{
						writer.WriteLine(report);
					}
				}
			}
			ViewModel.CrashReports = null;
		}

		private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			if (Debugger.IsAttached) Debugger.Break();
			Crash(e.Exception);
			e.Handled = true;
		}

		private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
		{
			if (Debugger.IsAttached) Debugger.Break();
			Crash(e.ExceptionObject);
			e.Handled = true;
		}

		#region Phone application initialization

		// Avoid double-initialization
		private bool phoneApplicationInitialized = false;

		// Do not add any additional code to this method
		private void InitializePhoneApplication()
		{
			if (phoneApplicationInitialized)
				return;

			// Create the frame but don't set it as RootVisual yet; this allows the splash
			// screen to remain active until the application is ready to render.
			RootFrame = new PhoneApplicationFrame();
			RootFrame.Navigated += CompleteInitializePhoneApplication;

			// Handle navigation failures
			RootFrame.NavigationFailed += RootFrame_NavigationFailed;

			// Handle reset requests for clearing the backstack
			RootFrame.Navigated += CheckForResetNavigation;

			// Ensure we don't initialize again
			phoneApplicationInitialized = true;
		}

		// Do not add any additional code to this method
		private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
		{
			// Set the root visual to allow the application to render
			if (RootVisual != RootFrame)
				RootVisual = RootFrame;

			// Remove this handler since it is no longer needed
			RootFrame.Navigated -= CompleteInitializePhoneApplication;
		}

		private void CheckForResetNavigation(object sender, NavigationEventArgs e)
		{
			// If the app has received a 'reset' navigation, then we need to check
			// on the next navigation to see if the page stack should be reset
			if (e.NavigationMode == NavigationMode.Reset)
				RootFrame.Navigated += ClearBackStackAfterReset;
		}

		private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
		{
			// Unregister the event so it doesn't get called again
			RootFrame.Navigated -= ClearBackStackAfterReset;

			// Only clear the stack for 'new' (forward) and 'refresh' navigations
			if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh)
				return;

			// For UI consistency, clear the entire page stack
			while (RootFrame.RemoveBackEntry() != null)
			{
				; // do nothing
			}
		}

		#endregion

		private void InitializeLanguage()
		{
			try
			{
				RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);
				FlowDirection flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
				RootFrame.FlowDirection = flow;
			}
			catch
			{
				if (Debugger.IsAttached)
				{
					Debugger.Break();
				}
				throw;
			}
		}
	}
}