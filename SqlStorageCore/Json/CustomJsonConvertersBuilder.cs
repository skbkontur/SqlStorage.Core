using System.Linq;

using Newtonsoft.Json;

namespace SkbKontur.SqlStorageCore.Json
{
    public static class CustomJsonConvertersBuilder
    {
        public static JsonConverter[] Build(JsonConverter[]? jsonConverters)
        {
            var timestampJsonConverter = new TimestampJsonConverter();
            return jsonConverters == null
                       ? new JsonConverter[] {timestampJsonConverter}
                       : jsonConverters.Append(timestampJsonConverter).ToArray();
        }
    }
}