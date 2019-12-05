using System;

using JetBrains.Annotations;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.SqlStorageCore.EventLog
{
    public class SqlEvent<TEntity>
    {
        public Guid EventId { get; set; }

        public SqlEventType EventType { get; set; }

        public long EventOffset { get; set; }

        public Timestamp EventTimestamp { get; set; }
        
        public TEntity EntitySnapshot { get; set; }
    }
}