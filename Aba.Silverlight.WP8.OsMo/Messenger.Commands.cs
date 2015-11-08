using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Aba.Silverlight.WP8.OsMo
{
	public partial class Messenger
	{
		private void ProcessReply(string line)
		{
			if (Regex.IsMatch(line, "[A-Z]+|.*"))
			{
				var pair = line.Split(new char[] { '|' }, 2);
				switch (pair[0])
				{
					case "INIT":
						CMd();
						break;
					case "MD":
						App.RootFrame.Dispatcher.BeginInvoke(() => { App.ViewModel.MessageOfTheDay = pair[1].Trim(); });
						break;
					default:
						break;
				}
			}
			else
			{
				// wtf?
			}
		}

		private void CInit()
		{
			Send("INIT", Token);
		}

		private void CMd()
		{
			Send("MD");
		}

	}
}
