using System;
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

		private void Application_Launching(object sender, LaunchingEventArgs e)
		{
			Messenger.Connect();
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

		// Code to execute if a navigation fails
		private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			if (Debugger.IsAttached)
			{
				// A navigation has failed; break into the debugger
				Debugger.Break();
			}
		}

		// Code to execute on Unhandled Exceptions
		private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
		{
			if (Debugger.IsAttached)
			{
				// An unhandled exception has occurred; break into the debugger
				Debugger.Break();
			}
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