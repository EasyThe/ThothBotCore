using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ThothBotCore.Utilities
{
    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
		public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
            return DateTime.ParseExact(reader.GetString(), "G", CultureInfo.CreateSpecificCulture("en-us"));
        }

		public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
		{
			//Don't implement this unless you're going to use the custom converter for serialization too
			throw new NotImplementedException();
		}
	}
}
