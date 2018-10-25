using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.EventLog
{
    public class SqlEventLogEntry : SqlEntity
    {
        [NotNull]
        public string EntityType { get; set; }

        [NotNull]
        public string EntityContent { get; set; }

        [NotNull]
        public string ModificationType { get; set; }

        public long Offset { get; set; }

        [NotNull]
        public Timestamp Timestamp { get; set; }
    }
}