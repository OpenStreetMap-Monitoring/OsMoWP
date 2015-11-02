using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Aba.Silverlight.WP8.OsMo.ViewModels
{
	public abstract class BaseModel : INotifyPropertyChanged
	{
		protected abstract string[] _ModelNotifyFields { get; }
		public event PropertyChangedEventHandler PropertyChanged;
		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "none passed")
		{
			if (null != PropertyChanged)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		protected void NotifyModelChanged()
		{
			foreach (var property in _ModelNotifyFields)
			{
				NotifyPropertyChanged(property);
			}
		}
	}

}
