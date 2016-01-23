using Aba.Silverlight.WP8.OsMo.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
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
			App.ViewModel.AddDebugLog(string.Format("<{0}", line));
			var pattern = "^([A-Z]+)([:][A-Z0-9_]+)?(|.+)?$";
			var match = Regex.Match(line, pattern);
			if (match.Success)
			{
				var command = match.Groups[1].Value;
				var parameter = string.IsNullOrEmpty(match.Groups[2].Value) ? null : match.Groups[2].Value.Substring(1);
				var addict = string.IsNullOrWhiteSpace(match.Groups[3].Value) ? null : match.Groups[3].Value.Substring(1).Trim();
				dynamic json = string.IsNullOrEmpty(addict) ? null
					: JsonConvert.DeserializeObject<dynamic>(addict, new JsonSerializerSettings { Error = (s, e) => { e.ErrorContext.Handled = true; } });
				if (json == null) json = JsonConvert.DeserializeObject<dynamic>("{}");

				switch (command)
				{
					case "BYE":
						Transport.Shutdown(SocketShutdown.Both);
						Transport.Close();
						break;
					case "G":
						foreach (var c in (json as JArray).Cast<JValue>().Select(s => s.Value.ToString().Split('|')).ToList())
						{
							Do(() => { App.ViewModel.GroupsModel.AddCoordinate(c[0], Coordinate.Create(c[1])); });
						}
						break;
					case "GA":
						CGroup();
						break;
					case "GD":
						CGroup();
						break;
					case "GE":
						CGroup();
						break;
					case "GP":
						var group = JsonConvert.DeserializeObject<Group>(addict);
						Do(() => { App.ViewModel.GroupsModel.SetUsers(parameter, group.Users); });
						CGpr(parameter);
						break;
					case "GPR":
						break;
					case "GROUP":
						Do(() => { lock (App.ViewModel.GroupsModel.Groups) { App.ViewModel.GroupsModel.Groups = JsonConvert.DeserializeObject<List<Group>>(addict); } });
						break;
					case "INIT":
						if (json["error"] != null)
						{
							Disconnect();
						}
						else
						{
							Do(() => { App.ViewModel.TrackerId = ((JValue)json["id"]).ToObject<string>(); });
							CMd();
						}
						break;
					case "MD":
						Status = ConnectionStatus.Connected;
						while (SendQueue.Count > 0)
						{
							Send(SendQueue.Dequeue());
						}
						Do(() => { App.ViewModel.MessageOfTheDay = addict; });
						break;
					case "P":
						break;
					case "PP":
						CP();
						break;
					case "RC":
						break;
					case "TO":
						Do(() => { App.ViewModel.SessionId = ((JValue)json["url"]).ToObject<string>(); });
						break;
					case "T":
						break;
					case "TC":
						Do(() => { App.ViewModel.SessionId = null; });
						break;
					default:
						if (Debugger.IsAttached) Debugger.Break();
						break;
				}
			}
		}

		private void Do(Action a)
		{
			App.RootFrame.Dispatcher.BeginInvoke(a);
		}

		#region Tcp api v2 commands

		public void CBye()
		{
			Send(new Message("BYE"));
		}

		public void CGa(string groupName)
		{
			Send(new Message("GA", groupName, null));
		}

		public void CGd(string groupName)
		{
			Send(new Message("GD", groupName, null));
		}

		public void CGe(string groupName, string displayName)
		{
			Send(new Message("GE", groupName, displayName));
		}

		public void CGpr(string groupName)
		{
			Send(new Message("GPR", groupName, null));
		}

		public void CGroup()
		{
			Send(new Message("GROUP"));
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
		public void CT(Coordinate coord)
		{
			if (coord == null) return;
			var addict = new StringBuilder();
			var format = new System.Globalization.NumberFormatInfo() { NumberDecimalDigits = 6, NumberDecimalSeparator = ".", NumberGroupSeparator = string.Empty };
			addict.AppendFormat("L{0}:{1}", coord.Lat.ToString("N6", format), coord.Lon.ToString("N6", format));
			addict.AppendFormat("S{0}A{1}H{2}C{3}T{4}", coord.Speed.ToString("N2", format), coord.Alt, coord.HDOP, coord.Course, coord.Timestamp);
			Send(new Message("T", addict.ToString()));
		}

		#endregion
	}
}
