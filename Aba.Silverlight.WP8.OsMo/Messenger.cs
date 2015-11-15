using Aba.Silverlight.WP8.OsMo.Models;
using Aba.Silverlight.WP8.OsMo.Resources;
using System;
using System.Collections.Generic;
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
		private int Serial { get; set; }
		private Queue<Message> SendQueue { get; set; }

		public bool Connected { get { return Transport.Connected; } }

		public Messenger()
		{
			ServiceHost = AppResources.MessengerHost;
			ApplicationId = WebUtility.UrlEncode(Resources.AppResources.MessengerApplicationId);
			DeviceId = WebUtility.UrlEncode(App.ViewModel.TrackerId);
			PlatformInfo = WebUtility.UrlEncode(string.Format("{0} / {1}", Environment.OSVersion.Platform, Environment.OSVersion.Version));
			Transport = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			SendQueue = new Queue<Message>();
		}

		public void Connect()
		{
			if (Transport.Connected || InProcess) return;
			InProcess = true;
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
			InProcess = false;
			Token = null;
			if (Transport.Connected)
			{
				CBye();
			}
		}

		private void GetToken(Action Success)
		{
			if (!IsolatedStorageSettings.ApplicationSettings.Contains(SERVER_DEVICE_ID)) return;
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
			client.DownloadStringAsync(new Uri(string.Format("{0}/init?app={1}&device={2}&serial={3}", ServiceHost, ApplicationId
				, IsolatedStorageSettings.ApplicationSettings[SERVER_DEVICE_ID], Serial++)));
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
				InProcess = false;
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
				InProcess = false;
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
			else if (!InProcess || InProcess && message.Command == "INIT" || InProcess && message.Command == "MD")
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
