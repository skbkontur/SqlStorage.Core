using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;
using SKBKontur.Catalogue.EDI.SqlStorageCore.EventLog;
using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.EventFeeds
{
    public class SqlStorageOffsetInterpreter<TEntity> : IOffsetInterpreter<long>
        where TEntity : IIdentifiableSqlEntity
    {
        public SqlStorageOffsetInterpreter(ISqlEventLogRepository<TEntity> eventLogRepository)
        {
            this.eventLogRepository = eventLogRepository;
        }

        public int Compare(long x, long y)
        {
            return x.CompareTo(y);
        }

        [NotNull]
        public string Format(long offset)
        {
            return offset.ToString();
        }

        [CanBeNull]
        public Timestamp GetTimestampFromOffset(long offset)
        {
            var offsetMaxDateTime = eventLogRepository.GetMaxTimestampForOffset(offset);
            return new Timestamp(offsetMaxDateTime.ToUniversalTime());
        }

        public long GetMaxOffsetForTimestamp(Timestamp timestamp)
        {
            return eventLogRepository.GetLastOffsetForTimestamp(timestamp.ToDateTime());
        }

        private readonly ISqlEventLogRepository<TEntity> eventLogRepository;
    }
}