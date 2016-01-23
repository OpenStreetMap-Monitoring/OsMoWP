using System;
using System.Linq;
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
		protected override string[] _ModelNotifyFields { get { return new string[] { "Groups", "Tracks" }; } }

		private List<Group> _Groups = new List<Group>();
		public List<Group> Groups { get { return _Groups; } set { if (value != _Groups) { _Groups = value; NotifyPropertyChanged(); } } }

		private Dictionary<string, List<Coordinate>> _Tracks;
		public Dictionary<string, List<Coordinate>> Tracks { get { return _Tracks; } set { if (value != _Tracks) { _Tracks = value; NotifyPropertyChanged(); } } }

		public void SetUsers(string groupId, List<GpUser> users)
		{
			var listGroup = this.Groups.FirstOrDefault(f => f.U == groupId);
			if (listGroup == null)
			{
				App.Messenger.CGroup();
				return;
			}
			lock (Groups)
			{
				foreach (var u in users)
				{
					var index = 0;
					while (index < listGroup.Users.Count && listGroup.Users[index].U != u.U) index++;
					if (index == listGroup.Users.Count)
					{
						listGroup.Users.Add(u);
					}
					else
					{
						listGroup.Users[index] = u;
					}
				}
			}
		}

		public void AddCoordinate(string userId, Coordinate coordinate)
		{
			if (Tracks == null) Tracks = new Dictionary<string, List<Coordinate>>();
			if (!Tracks.ContainsKey(userId))
			{
				Tracks[userId] = new List<Coordinate>();
			}
			Tracks[userId].Add(coordinate);
			NotifyPropertyChanged("Tracks");
		}
	}
}