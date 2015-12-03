using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using System;
using Microsoft.Phone.Shell;

namespace Aba.Silverlight.WP8.OsMo.GpsTask
{
	public class ScheduledAgent : ScheduledTaskAgent
	{
		public const string PERIODIC_TASK_NAME = "Aba.Silverlight.WP8.OsMo.GpsTask.PeriodicTask";

		static ScheduledAgent()
		{
			Deployment.Current.Dispatcher.BeginInvoke(delegate
			{
				Application.Current.UnhandledException += UnhandledException;
			});
		}

		private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
		{
			if (Debugger.IsAttached)
			{
				Debugger.Break();
			}
		}

		protected override void OnInvoke(ScheduledTask task)
		{
#if DEBUG
			ScheduledActionService.LaunchForTest(task.Name, new TimeSpan(0, 0, 5));
			ShellToast toast = new ShellToast();
			toast.Title = "t";
			toast.Content = "tt";
			toast.Show();
#endif
			NotifyComplete();
		}
	}
}