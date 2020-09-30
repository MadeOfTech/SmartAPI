using Newtonsoft.Json;
using System;

namespace MadeOfTech.SmartAPI.JsonSerialization
{
	internal class DateTimeISO8601Converter : JsonConverter
	{
		public override bool CanConvert(Type objectType) => objectType == typeof(DateTime);
		public override bool CanRead => false;
		public override bool CanWrite => true;
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => throw new NotImplementedException();
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue(((DateTime)value).ToString("yyyyMMddTHHmmssfffZ"));
		}
	}
}
