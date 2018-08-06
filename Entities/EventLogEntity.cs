using System;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Entities
{
    public class EventLogEntity : IdentifiableEntity
    {
        [NotNull]
        public string EntityType { get; set; }

        [NotNull]
        public string EntityContent { get; set; }

        [NotNull]
        public string Type { get; set; }

        public long Offset { get; set; }

        public DateTime Timestamp { get; set; }
    }
}