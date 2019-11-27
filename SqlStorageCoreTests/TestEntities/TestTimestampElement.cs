using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.SqlStorageCore.Tests.TestEntities
{
    public class TestTimestampElement : SqlEntity
    {
        public Timestamp Timestamp { get; set; }
    }
}