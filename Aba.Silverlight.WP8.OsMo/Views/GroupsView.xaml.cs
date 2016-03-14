using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Aba.Silverlight.WP8.OsMo.Models;

namespace Aba.Silverlight.WP8.OsMo.Views
{
	public partial class GroupsView : UserControl
	{
		public GroupsView()
		{
			InitializeComponent();
		}

		private void AddButton_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(GroupName.Text) || string.IsNullOrEmpty(DisplayName.Text)) return;
			App.Messenger.CGe(GroupName.Text, DisplayName.Text);
			GroupName.Text = string.Empty;
			DisplayName.Text = string.Empty;
			AddPanel.Visibility = Visibility.Collapsed;
			OpenAddPanelButton.Visibility = Visibility.Visible;
		}

		private void CloseAddPanelButton_Click(object sender, RoutedEventArgs e)
		{
			AddPanel.Visibility = Visibility.Collapsed;
			OpenAddPanelButton.Visibility = Visibility.Visible;
		}

		private void OpenAddPanelButton_Click(object sender, RoutedEventArgs e)
		{
			AddPanel.Visibility = Visibility.Visible;
			OpenAddPanelButton.Visibility = Visibility.Collapsed;
		}

		private void IsActive_Changed(object sender, RoutedEventArgs e)
		{
			var checkbox = sender as CheckBox;
			var item = checkbox.DataContext as Group;
			if (!checkbox.IsEnabled) checkbox.IsEnabled = true;
			if (checkbox.IsChecked.GetValueOrDefault() == item.Active) return;
			checkbox.IsEnabled = false;
			if (checkbox.IsChecked.GetValueOrDefault())
			{
				App.Messenger.CGa(item.U);
			}
			else
			{
				App.Messenger.CGd(item.U);
			}
		}
	}
}
