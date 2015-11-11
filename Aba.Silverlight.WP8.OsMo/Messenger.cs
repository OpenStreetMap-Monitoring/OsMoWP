using Aba.Silverlight.WP8.OsMo.Models;
using Aba.Silverlight.WP8.OsMo.Resources;
using System;
using System.IO.IsolatedStorage;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Aba.Silverlight.WP8.OsMo
{
	public partial class Messenger
	{
		private const string SERVER_DEVICE_ID = "SERVER_DEVICE_ID";

		private Socket Transport { get; set; }

		private string ServiceHost { get; set; }
		private string DataHost { get; set; }
		private int DataPort { get; set; }
		private string ApplicationId { get; set; }
		private string DeviceId { get; set; }
		private string PlatformInfo { get; set; }
		private bool InProcess { get; set; }
		private string Token { get; set; }

		public bool Connected { get { return Transport.Connected; } }

		public Messenger()
		{
			ServiceHost = AppResources.MessengerHost;
			ApplicationId = WebUtility.UrlEncode(Resources.AppResources.MessengerApplicationId);
			DeviceId = WebUtility.UrlEncode(App.ViewModel.DeviceId);
			PlatformInfo = WebUtility.UrlEncode(string.Format("{0} / {1}", Environment.OSVersion.Platform, Environment.OSVersion.Version));
			Transport = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

		public void Connect()
		{
			if (Transport.Connected || InProcess) return;
			if (IsolatedStorageSettings.ApplicationSettings.Contains(SERVER_DEVICE_ID))
			{
				GetToken(() => { Init(); });
			}
			else
			{
				RegisterDevice(() => { GetToken(() => { Init(); }); });
			}
		}

		void Transport_Connected(object sender, SocketAsyncEventArgs e)
		{
			e.Completed -= Transport_Connected;
			e.Dispose();
			CInit();
			var receive = new SocketAsyncEventArgs();
			receive.SetBuffer(new byte[16384], 0, 16384);
			receive.Completed += Transport_Received;
			Transport.ReceiveAsync(receive);
		}

		void Transport_Sent(object sender, SocketAsyncEventArgs e)
		{
			e.Completed -= Transport_Sent;
			e.Dispose();
		}

		void Transport_Received(object sender, SocketAsyncEventArgs e)
		{
			if (e.SocketError != SocketError.Success) return;
			ProcessReply(Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred));
			e.Completed -= Transport_Received;
			e.Dispose();
			var args = new SocketAsyncEventArgs();
			args.SetBuffer(new byte[16384], 0, 16384);
			args.Completed += Transport_Received;
			Transport.ReceiveAsync(args);
		}

		public void Disconnect()
		{
			Transport.Shutdown(SocketShutdown.Both);
			Transport.Close();
		}

		private void GetToken(Action Success)
		{
			if (Transport.Connected || InProcess || !IsolatedStorageSettings.ApplicationSettings.Contains(SERVER_DEVICE_ID)) return;
			InProcess = true;
			var client = new WebClient();
			client.DownloadStringCompleted += (s, e) =>
			{
				Token = Regex.Match(e.Result, "[\"]token[\"][:][\"]([^\"]+)[\"]").Groups[1].Value;
				var address = Regex.Match(e.Result, "[\"]address[\"][:][\"]([^\"]+)[\"]").Groups[1].Value.Split(':');
				DataHost = address[0];
				DataPort = Convert.ToInt32(address[1]);
				InProcess = false;
				Success();
			};
			client.DownloadStringAsync(new Uri(string.Format("{0}/init?app={1}&device={2}", ServiceHost, ApplicationId, IsolatedStorageSettings.ApplicationSettings[SERVER_DEVICE_ID])));
		}

		private void Init()
		{
			if (Transport.Connected || InProcess || string.IsNullOrEmpty(Token)) return;
			var args = new SocketAsyncEventArgs();
			args.RemoteEndPoint = new DnsEndPoint(DataHost, DataPort);
			args.Completed += Transport_Connected;
			Transport.ConnectAsync(args);
		}

		private void RegisterDevice(Action Success)
		{
			if (Transport.Connected || InProcess || IsolatedStorageSettings.ApplicationSettings.Contains(SERVER_DEVICE_ID)) return;
			InProcess = true;
			var client = new WebClient();
			client.DownloadStringCompleted += (s, e) =>
			{
				var device = Regex.Match(e.Result, "[\"]device[\"][:][\"]([^\"]+)[\"]").Groups[1].Value;
				IsolatedStorageSettings.ApplicationSettings[SERVER_DEVICE_ID] = WebUtility.UrlEncode(device);
				IsolatedStorageSettings.ApplicationSettings.Save();
				InProcess = false;
				Success();
			};
			client.DownloadStringAsync(new Uri(string.Format("{0}/new?app={1}&id={2}&imei={3}&platform={4}", ServiceHost, ApplicationId, DeviceId, 0, PlatformInfo)));
		}

		private void Send(Message message)
		{
			//TODO: make reconnect and queue
			var line = new StringBuilder(message.Command);
			if (!string.IsNullOrEmpty(message.Parameter))
			{
				line.Append(':');
				line.Append(message.Parameter);
			}
			if (!string.IsNullOrEmpty(message.Addict))
			{
				line.Append('|');
				line.Append(message.Addict);
			}
			line.Append('\n');
			var args = new SocketAsyncEventArgs();
			var buffer = Encoding.UTF8.GetBytes(line.ToString());
			args.SetBuffer(buffer, 0, buffer.Length);
			args.Completed += Transport_Sent;
			Transport.SendAsync(args);
		}
	}
}
