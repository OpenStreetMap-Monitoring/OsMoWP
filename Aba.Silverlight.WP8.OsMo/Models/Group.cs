using Aba.Silverlight.WP8.OsMo.Converters;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Aba.Silverlight.WP8.OsMo.Models
{
	public class Group
	{
		public string U { get; set; }
		public string Id { get; set; }
		public string Name { get; set; }
		[JsonConverter(typeof(JsonBooleanConverter))]
		public bool Active { get; set; }
		public List<GpUser> Users { get; set; }
	}
}
