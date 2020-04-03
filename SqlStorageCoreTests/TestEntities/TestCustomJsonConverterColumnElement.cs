using System;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

using SkbKontur.SqlStorageCore.Schema;

namespace SkbKontur.SqlStorageCore.Tests.TestEntities
{
    public class TestCustomJsonConverterSqlEntity : SqlEntity
    {
        [JsonColumn]
        public TestCustomJsonConverterColumnElement CustomJsonColumn { get; set; }
    }

    public class TestCustomJsonConverterColumnElement
    {
        public TestCustomJsonConverterColumnElement(int intProperty, string stringProperty)
        {
            IntProperty = intProperty;
            StringProperty = stringProperty;
        }

        public int IntProperty { get; }
        public string StringProperty { get; }
    }

    public class TestCustomJsonConverterSqlEntryJsonConverter : JsonConverter<TestCustomJsonConverterColumnElement>
    {
        public const string FieldsDelimiter = "~:~";

        public override void WriteJson(JsonWriter writer, TestCustomJsonConverterColumnElement value, JsonSerializer serializer)
        {
            writer.WriteValue($"{value.IntProperty}{FieldsDelimiter}{value.StringProperty}");
        }

        public override TestCustomJsonConverterColumnElement ReadJson(JsonReader reader, Type objectType, TestCustomJsonConverterColumnElement existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return reader.TokenType switch
                {
                    JsonToken.String => TryParse(reader.Value?.ToString() ?? string.Empty),
                    _ => throw new JsonSerializationException("Unexpected token when parsing")
                };
        }

        private static TestCustomJsonConverterColumnElement TryParse(string jsonString)
        {
            var pattern = $@"(\d+)?{FieldsDelimiter}(.*)";
            var match = Regex.Match(jsonString, pattern);
            if (!match.Success)
                throw new JsonSerializationException("Unexpected token when parsing");
            return new TestCustomJsonConverterColumnElement(int.Parse(match.Groups[1].Value), match.Groups[2].Value);
        }
    }
}