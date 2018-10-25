using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.EventLog
{
    public class SqlEvent<TEntity>
    {
        public Guid EventId { get; set; }

        public SqlEventType EventType { get; set; }

        public long EventOffset { get; set; }

        [NotNull]
        public Timestamp EventTimestamp { get; set; }

        [NotNull]
        public TEntity EntitySnapshot { get; set; }
    }
}