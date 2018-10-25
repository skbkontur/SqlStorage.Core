using JetBrains.Annotations;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Schema
{
    public static class SqlCommonQueriesBuilder
    {
        public static string TicksFromTimestamp([NotNull] string timestampQuery)
        {
            return $"(({unixEpochSeconds} + extract(epoch from {timestampQuery} at time zone \'UTC\')) * 10 * 1000 * 1000)::bigint";
        }

        public static string TimestampToTicks([NotNull] string longQuery)
        {
            return $"to_timestamp({longQuery}::double precision / (10 * 1000 * 1000) - {unixEpochSeconds})";
        }

        private const string unixEpochSeconds = "62135596800";
    }
}