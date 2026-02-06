using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KiteConnect
{
    internal class CandleJsonConverter : JsonConverter<Candle>
    {
        public override Candle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Expected StartArray token.");

            var candle = new Candle();

            reader.Read();
            candle.TimeStamp = DateTime.ParseExact(reader.GetString(), "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

            reader.Read();
            candle.Open = reader.GetDecimal();

            reader.Read();
            candle.High = reader.GetDecimal();

            reader.Read();
            candle.Low = reader.GetDecimal();

            reader.Read();
            candle.Close = reader.GetDecimal();

            reader.Read();
            candle.Volume = reader.GetUInt64();

            reader.Read();
            if (reader.TokenType == JsonTokenType.EndArray)
                return candle;
            else
                candle.OI = reader.GetUInt64();

            reader.Read();
            if (reader.TokenType != JsonTokenType.EndArray)
                throw new JsonException("Expected EndArray token.");

            return candle;
        }

        public override void Write(Utf8JsonWriter writer, Candle value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
