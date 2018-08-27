using System.Linq;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;
using SKBKontur.Catalogue.EDI.SqlStorageCore.EventLog;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.EventFeeds
{
    public class SqlEventSource<TEntity> : IEventSource<SqlEvent<TEntity>, long>
        where TEntity : IIdentifiableSqlEntity
    {
        public SqlEventSource(ISqlEventLogRepository<TEntity> sqlEventLogRepository)
        {
            this.sqlEventLogRepository = sqlEventLogRepository;
        }

        [NotNull]
        public string GetDescription()
        {
            return $"EntityEventLog event source for event log entity {typeof(TEntity).Name}";
        }

        [NotNull]
        public EventsQueryResult<SqlEvent<TEntity>, long> GetEvents(long fromOffsetExclusive, long toOffsetInclusive, int estimatedCount)
        {
            var events = sqlEventLogRepository
                .GetEvents(fromOffsetExclusive, toOffsetInclusive, estimatedCount)
                .Select(e => new EventWithOffset<SqlEvent<TEntity>, long>(e, e.EventOffset))
                .ToList();

            var totalCount = sqlEventLogRepository.GetCount(fromOffsetExclusive, toOffsetInclusive);

            return new EventsQueryResult<SqlEvent<TEntity>, long>(
                events,
                events.Last().Offset,
                totalCount <= events.Count);
        }

        private readonly ISqlEventLogRepository<TEntity> sqlEventLogRepository;
    }
}