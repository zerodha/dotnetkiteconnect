using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KiteConnect
{
    internal class CustomDateTimeJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (value.Length == 10)
                return DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            else if (value.Length == 16)
                return DateTime.ParseExact(value, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            else
                return DateTime.ParseExact(value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
