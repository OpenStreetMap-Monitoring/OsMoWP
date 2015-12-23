using Aba.Silverlight.WP8.OsMo.Models;
using Aba.Silverlight.WP8.OsMo.Resources;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using Windows.Phone.System.Analytics;

namespace Aba.Silverlight.WP8.OsMo
{
	public partial class Messenger
	{
		public enum ConnectionStatus { NotConnected = 0, Connecting = 1, Connected = 2 }

		public const string SERVER_DEVICE_ID = "SERVER_DEVICE_ID";
		public const string SERVER_USER_KEY = "SERVER_USER_KEY";
		private const string MESSENGER_APPLICATION_ID = "U33_Kqrt52pN";
		private const string MESSENGER_HOST = "https://api.osmo.mobi";

		private Socket Transport { get; set; }
		private string ServiceHost { get; set; }
		private string DataHost { get; set; }
		private int DataPort { get; set; }
		private string ApplicationId { get; set; }
		private string DeviceId { get; set; }
		private string PlatformInfo { get; set; }

		private ConnectionStatus _Status;
		private ConnectionStatus Status { get { return _Status; } set { _Status = value; Do(() => { App.ViewModel.MessengerStatus = value; }); } }
		private string Token { get; set; }
		private int Serial { get; set; }
		private Queue<Message> SendQueue { get; set; }
		private DispatcherTimer Pinger { get; set; }

		public Messenger()
		{
			ServiceHost = MESSENGER_HOST;
			ApplicationId = WebUtility.UrlEncode(MESSENGER_APPLICATION_ID);
			DeviceId = WebUtility.UrlEncode(HostInformation.PublisherHostId);
			PlatformInfo = WebUtility.UrlEncode(string.Format("{0} / {1}", Environment.OSVersion.Platform, Environment.OSVersion.Version));
			Transport = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			SendQueue = new Queue<Message>();
			Pinger = new DispatcherTimer() { Interval = new TimeSpan(0, 1, 0) };
			Pinger.Tick += Pinger_Tick;
			Pinger.Start();
		}

		void Pinger_Tick(object sender, EventArgs e)
		{
			if (App.ViewModel.SettingsModel.PersistentConnection.GetValueOrDefault())
			{
				CP();
			}
		}

		public void Connect()
		{
			if (Transport.Connected || Status == ConnectionStatus.Connecting) return;
			Status = ConnectionStatus.Connecting;
			App.ViewModel.AddDebugLog("!Connecting");
			if (IsolatedStorageSettings.ApplicationSettings.Contains(SERVER_DEVICE_ID))
			{
				GetToken(() => { Init(); });
			}
			else
			{
				RegisterDevice(() => { GetToken(() => { Init(); }); });
			}
		}

		public void Disconnect()
		{
			Token = null;
			if (Transport.Connected)
			{
				CBye();
			}
			Status = ConnectionStatus.NotConnected;
			App.ViewModel.AddDebugLog("!Disconnected");
		}

		private void GetToken(Action Success)
		{
			var iss = IsolatedStorageSettings.ApplicationSettings;
			if (!iss.Contains(SERVER_DEVICE_ID)) return;
			var client = new WebClient();
			client.DownloadStringCompleted += (s, e) =>
			{
				if (e.Error == null)
				{
					Token = Regex.Match(e.Result, "[\"]token[\"][:][\"]([^\"]+)[\"]").Groups[1].Value;
					var address = Regex.Match(e.Result, "[\"]address[\"][:][\"]([^\"]+)[\"]").Groups[1].Value.Split(':');
					DataHost = address[0];
					DataPort = Convert.ToInt32(address[1]);
					Success();
				}
				else
				{
					Disconnect();
				}
			};
			client.DownloadStringAsync(new Uri(string.Format("{0}/init?app={1}&device={2}&serial={3}{4}", ServiceHost, ApplicationId
				, iss[SERVER_DEVICE_ID], Serial++, iss.Contains(SERVER_USER_KEY) ? string.Format("&user={0}", iss[SERVER_USER_KEY]) : null)));
		}

		private void RegisterDevice(Action Success)
		{
			if (IsolatedStorageSettings.ApplicationSettings.Contains(SERVER_DEVICE_ID)) return;
			var client = new WebClient();
			client.DownloadStringCompleted += (s, e) =>
			{
				if (e.Error == null)
				{
					var device = Regex.Match(e.Result, "[\"]device[\"][:][\"]([^\"]+)[\"]").Groups[1].Value;
					lock (IsolatedStorageSettings.ApplicationSettings)
					{
						IsolatedStorageSettings.ApplicationSettings[SERVER_DEVICE_ID] = WebUtility.UrlEncode(device);
						IsolatedStorageSettings.ApplicationSettings.Save();
					}
					Success();
				}
				else
				{
					Disconnect();
				}
			};
			client.DownloadStringAsync(new Uri(string.Format("{0}/new?app={1}&id={2}&imei={3}&platform={4}", ServiceHost, ApplicationId, DeviceId, 0, PlatformInfo)));
		}

		private void Init()
		{
			if (string.IsNullOrEmpty(Token))
			{
				Disconnect();
				return;
			}
			var args = new SocketAsyncEventArgs();
			args.RemoteEndPoint = new DnsEndPoint(DataHost, DataPort);
			args.Completed += Transport_Connected;
			Transport.ConnectAsync(args);
		}

		void Transport_Connected(object sender, SocketAsyncEventArgs e)
		{
			var error = e.SocketError;
			e.Completed -= Transport_Connected;
			e.Dispose();
			if (error != SocketError.Success)
			{
				Disconnect();
				return;
			}
			CInit();
			var receive = new SocketAsyncEventArgs();
			receive.SetBuffer(new byte[16384], 0, 16384);
			receive.Completed += Transport_Received;
			Transport.ReceiveAsync(receive);
		}

		void Transport_Sent(object sender, SocketAsyncEventArgs e)
		{
			var error = e.SocketError;
			e.Completed -= Transport_Sent;
			e.Dispose();
			if (error != SocketError.Success)
			{
				Disconnect();
			}
		}

		void Transport_Received(object sender, SocketAsyncEventArgs e)
		{
			if (e.SocketError != SocketError.Success)
			{
				e.Completed -= Transport_Received;
				e.Dispose();
				Disconnect();
				return;
			}
			ProcessReply(Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred));
			e.Completed -= Transport_Received;
			e.Dispose();
			if (!Transport.Connected)
			{
				Disconnect();
				return;
			}
			var args = new SocketAsyncEventArgs();
			args.SetBuffer(new byte[16384], 0, 16384);
			args.Completed += Transport_Received;
			Transport.ReceiveAsync(args);
		}

		private void Send(Message message)
		{
			if (!Transport.Connected)
			{
				SendQueue.Enqueue(message);
				Connect();
			}
			else if (Status != ConnectionStatus.Connecting || Status == ConnectionStatus.Connecting && (message.Command == "INIT" || message.Command == "MD"))
			{
				var args = new SocketAsyncEventArgs();
				var buffer = Encoding.UTF8.GetBytes(message.ToString());
				args.SetBuffer(buffer, 0, buffer.Length);
				args.Completed += Transport_Sent;
				Transport.SendAsync(args);
				App.ViewModel.AddDebugLog(string.Format(">{0}", message.ToString()));
			}
			else
			{
				SendQueue.Enqueue(message);
			}
		}
	}
}
