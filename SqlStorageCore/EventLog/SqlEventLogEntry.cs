using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.SqlStorageCore.EventLog
{
    public class SqlEventLogEntry : SqlEntity
    {
        public string EntityType { get; set; }

        public string EntityContent { get; set; }

        public string ModificationType { get; set; }

        public long Offset { get; set; }

        public long TransactionId { get; set; }

        public Timestamp Timestamp { get; set; }
    }
}