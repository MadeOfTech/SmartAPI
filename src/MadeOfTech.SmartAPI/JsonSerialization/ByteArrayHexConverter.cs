using Newtonsoft.Json;
using System;
using System.Linq;

namespace MadeOfTech.SmartAPI.JsonSerialization
{
	internal class ByteArrayHexConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType) => objectType == typeof(byte[]);

		public override bool CanRead => false;
		public override bool CanWrite => true;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => throw new NotImplementedException();

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(((byte[])value).Hex());
		}
	}
}
