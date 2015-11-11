using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aba.Silverlight.WP8.OsMo.Converters
{
	public class JsonBooleanConverter : JsonConverter
	{
		public override bool CanWrite { get { return false; } }

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var value = reader.Value;

			if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
			{
				return false;
			}

			if (value.ToString() == "1")
			{
				return true;
			}
			return false;
		}

		public override bool CanConvert(Type objectType)
		{
			if (objectType == typeof(string) || objectType == typeof(bool))
			{
				return true;
			}
			return false;
		}
	}

}
