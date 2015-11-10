using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Aba.Silverlight.WP8.OsMo.Resources;
using Windows.Devices.Geolocation;
using Windows.Phone.System.Analytics;
using System.Collections.Generic;
using Aba.Silverlight.WP8.OsMo.Models;

namespace Aba.Silverlight.WP8.OsMo.ViewModels
{
	public class GroupsModel : BaseModel
	{
		protected override string[] _ModelNotifyFields { get { return new string[] { }; } }

		private IEnumerable<Group> _Groups;
		public IEnumerable<Group> Groups { get { return _Groups; } set { if (value != _Groups) { _Groups = value; NotifyPropertyChanged(); } } }

	}
}