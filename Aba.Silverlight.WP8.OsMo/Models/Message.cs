using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aba.Silverlight.WP8.OsMo.Models
{
	public class Message
	{
		public string Command { get; set; }
		public string Parameter { get; set; }
		public string Addict { get; set; }

		public Message(string command)
		{
			this.Command = command;
		}

		public Message(string command, string addict)
		{
			this.Command = command;
			this.Addict = addict;
		}
	}
}
