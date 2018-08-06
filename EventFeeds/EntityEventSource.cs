using System.Linq;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;
using SKBKontur.Catalogue.EDI.SqlStorageCore.EventLog;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.EventFeeds
{
    internal class EntityEventSource<TEntity> : IEventSource<EntityEvent<TEntity>, long>
        where TEntity : IIdentifiableEntity
    {
        public EntityEventSource(IEntitiesEventLogRepository<TEntity> entitiesEventLogRepository)
        {
            this.entitiesEventLogRepository = entitiesEventLogRepository;
        }

        [NotNull]
        public string GetDescription()
        {
            return $"EntityEventLog event source for event log entity {typeof(TEntity).Name}";
        }

        [NotNull]
        public EventsQueryResult<EntityEvent<TEntity>, long> GetEvents(long fromOffsetExclusive, long toOffsetInclusive, int estimatedCount)
        {
            var events = entitiesEventLogRepository
                .GetEvents(fromOffsetExclusive, toOffsetInclusive, estimatedCount)
                .Select(e => new EventWithOffset<EntityEvent<TEntity>, long>(e, e.EventOffset))
                .ToList();

            var totalCount = entitiesEventLogRepository.GetCount(fromOffsetExclusive, toOffsetInclusive);

            return new EventsQueryResult<EntityEvent<TEntity>, long>(
                events,
                events.Last().Offset,
                totalCount <= events.Count);
        }

        private readonly IEntitiesEventLogRepository<TEntity> entitiesEventLogRepository;
    }
}