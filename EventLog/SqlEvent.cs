using System;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.EventLog
{
    public class SqlEvent<TEntity>
    {
        public Guid EventId { get; set; }
        public TEntity EntitySnapshot { get; set; }
        public long EventOffset { get; set; }
        public SqlEventType EventType { get; set; }
    }
}