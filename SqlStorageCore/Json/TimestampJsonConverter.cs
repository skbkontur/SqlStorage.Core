using System;

using Newtonsoft.Json;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.SqlStorageCore.Json
{
    internal class TimestampJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteNull();
            else
                writer.WriteValue(((Timestamp)value).ToDateTime());
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.TokenType switch
                {
                    JsonToken.Null => null,
                    JsonToken.Date => new Timestamp((DateTime)reader.Value),
                    JsonToken.Integer => new Timestamp((long)reader.Value),
                    _ => throw new JsonSerializationException($"Unexpected token when parsing timestamp. Expected Date or Integer with value type long, got {reader.TokenType}")
                };
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Timestamp);
        }
    }
}