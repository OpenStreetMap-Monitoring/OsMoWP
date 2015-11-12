using Aba.Silverlight.WP8.OsMo.Converters;
using Newtonsoft.Json;

namespace Aba.Silverlight.WP8.OsMo.Models
{
	public class Group
	{
		public string Id { get; set; }
	
		public string Name { get; set; }

		[JsonConverter(typeof(JsonBooleanConverter))]
		public bool Active { get; set; }
	}
}
