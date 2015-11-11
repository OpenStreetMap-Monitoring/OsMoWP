using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

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
			if(string.IsNullOrEmpty(GroupName.Text) || string.IsNullOrEmpty(DisplayName.Text)) return;
			App.Messenger.CGe(GroupName.Text, DisplayName.Text);
			GroupName.Text = string.Empty;
			DisplayName.Text = string.Empty;
		}
	}
}
