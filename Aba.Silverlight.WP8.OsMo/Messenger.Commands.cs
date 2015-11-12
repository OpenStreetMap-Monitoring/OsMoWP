using Aba.Silverlight.WP8.OsMo.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Devices.Geolocation;
using Group = Aba.Silverlight.WP8.OsMo.Models.Group;

namespace Aba.Silverlight.WP8.OsMo
{
	public partial class Messenger
	{

		private void ProcessReply(string line)
		{
			var pattern = "^([A-Z]+)([:][A-Z0-9_]+)?(|.+)?$";
			var match = Regex.Match(line.Trim(), pattern);
			if (match.Success)
			{
				var command = match.Groups[1].Value;
				var parameter = string.IsNullOrEmpty(match.Groups[2].Value) ? null : match.Groups[2].Value.Substring(1);
				var addict = string.IsNullOrWhiteSpace(match.Groups[3].Value) ? null : match.Groups[3].Value.Substring(1).Trim();

				switch (command)
				{
					case "GE":
						CGroup();
						break;
					case "GP":
						if (Debugger.IsAttached) Debugger.Break();
						break;
					case "GROUP":
						Do(() =>
						{
							App.ViewModel.GroupsModel.Groups = JsonConvert.DeserializeObject<IEnumerable<Group>>(addict);
						});
						break;
					case "INIT":
						CMd();
						break;
					case "MD":
						Do(() => { App.ViewModel.MessageOfTheDay = addict; });
						break;
					case "P":
						if (Debugger.IsAttached) Debugger.Break();
						break;
					case "PP":
						if (Debugger.IsAttached) Debugger.Break();
						CP();
						break;
					case "RC":
						break;
					case "TO":
						break;
					case "T":
						break;
					case "TC":
						break;
					default:
						if (Debugger.IsAttached) Debugger.Break();
						break;
				}
			}
		}

		private void Do(Action a)
		{
			if (App.RunningInBackground)
			{
				a();
			}
			else
			{
				App.RootFrame.Dispatcher.BeginInvoke(a);
			}
		}

		#region Tcp api v2 commands

		public void CGroup()
		{
			Send(new Message("GROUP"));
		}

		public void CGe(string groupName, string displayName)
		{
			Send(new Message("GE", groupName, displayName));
		}

		/// <summary>
		/// Inits tcp connection
		/// </summary>
		private void CInit()
		{
			Send(new Message("INIT", Token));
		}

		/// <summary>
		/// Gets message of the day
		/// </summary>
		public void CMd()
		{
			Send(new Message("MD"));
		}

		/// <summary>
		/// Send pong
		/// </summary>
		public void CP()
		{
			Send(new Message("P"));
		}

		/// <summary>
		/// Opens tracking session
		/// </summary>
		public void CTo()
		{
			Send(new Message("TO"));
		}

		/// <summary>
		/// Closes tracking session
		/// </summary>
		public void CTc()
		{
			Send(new Message("TC"));
		}

		/// <summary>
		/// Send tracking data
		/// </summary>
		/// <param name="coord">Gps point</param>
		public void CT(Geocoordinate coord)
		{
			if (coord == null) return;
			var addict = new StringBuilder();
			var format = CultureInfo.InvariantCulture.NumberFormat;
			addict.AppendFormat("L{0}:{1}", coord.Latitude.ToString(format), coord.Longitude.ToString(format));
			addict.AppendFormat("S{0}A{1}", coord.Speed.GetValueOrDefault().ToString(format), Convert.ToInt32(coord.Altitude.GetValueOrDefault()).ToString(format));
			addict.AppendFormat("H{0}C{1}", coord.Accuracy.ToString(format), coord.Heading.GetValueOrDefault().ToString(format));
			addict.AppendFormat("T{0}", Convert.ToInt32(coord.Timestamp.UtcDateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString(format));
			Send(new Message("T", addict.ToString()));
		}

		#endregion
	}
}
