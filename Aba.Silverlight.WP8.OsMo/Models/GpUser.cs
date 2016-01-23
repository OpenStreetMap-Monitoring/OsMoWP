using Aba.Silverlight.WP8.OsMo.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aba.Silverlight.WP8.OsMo.Models
{
	public class GpUser
	{
		//{"u":"1552","name":"aba","color":"#3ea85b","state":1,"group_tracker_id":"b9c24571dad14d71","time":"2016-01-19 10:58:25"}
		public string U { get; set; }
		public string Name { get; set; }
		public string Color { get; set; }
		public double[][] Track { get; set; }
	}
}
