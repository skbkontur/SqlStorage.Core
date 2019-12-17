namespace SkbKontur.SqlStorageCore.Schema
{
    internal static class SqlCommonQueriesBuilder
    {
        public static string TicksFromTimestamp(string timestampQuery)
        {
            return $"(({unixEpochSeconds} + extract(epoch from {timestampQuery} at time zone \'UTC\')) * 10 * 1000 * 1000)::bigint";
        }

        public static string CurrentTransactionId() => "txid_current()";

        public static string CurrentTransactionTimestamp() => "transaction_timestamp()";

        private const string unixEpochSeconds = "62135596800";
    }
}