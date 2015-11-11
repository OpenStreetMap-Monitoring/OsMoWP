using Aba.Silverlight.WP8.OsMo.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aba.Silverlight.WP8.OsMo.Models
{
	public class Group
	{

		//[{"u":"2588","type":"0","gu":"2894","nick":"e80","color":"#3ea85b","url":"bJUNGgH","name":"moto","description":""
		// ,"active":"1","block":"0","policy":"","track":[],"point":[]
		// ,"users":[{"u":"1552","name":"aba","connected":"2015-11-11 10:33:16","color":"#3ea85b","state":"0"}]
		// ,"id":"CCR_1256"}]

		public string Id { get; set; }
	
		public string Name { get; set; }

		[JsonConverter(typeof(JsonBooleanConverter))]
		public bool Active { get; set; }
	}
}
