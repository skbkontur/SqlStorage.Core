using System;
using System.Linq;

using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using SKBKontur.Catalogue.EDI.SqlStorageCore.DatabaseContext;
using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;
using SKBKontur.Catalogue.EDI.SqlStorageCore.Storage;
using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.EventLog
{
    [UsedImplicitly]
    public class EntitiesEventLogRepository<TEntity> : IEntitiesEventLogRepository<TEntity>
        where TEntity : IIdentifiableEntity
    {
        public EntitiesEventLogRepository(IEntityStorage<EventLogEntity> eventLogEntityStorage, Func<EntitiesDatabaseContext> createDbContext)
        {
            this.eventLogEntityStorage = eventLogEntityStorage;
            var entityType = typeof(TEntity);
            entityTypeName = GetEventLogEntityTypeName(createDbContext, entityType);
        }

        [NotNull]
        private static string GetEventLogEntityTypeName([NotNull] Func<EntitiesDatabaseContext> createDbContext, [NotNull] Type entityType)
        {
            using (var context = createDbContext())
            {
                var name = context.Model.FindEntityType(entityType)?.Relational()?.TableName;
                if (string.IsNullOrEmpty(name))
                    throw new InvalidProgramStateException($"EventLog entity type not found for {entityType.Name}");
                return name;
            }
        }

        [NotNull, ItemNotNull]
        public EntityEvent<TEntity>[] GetEvents(long fromOffsetExclusive, long toOffsetInclusive, int limit)
        {
            return eventLogEntityStorage
                .Find(
                    e => e.Offset > fromOffsetExclusive
                         && e.Offset <= toOffsetInclusive
                         && e.EntityType == entityTypeName,
                    0, limit)
                .Select(BuildEntityEvent)
                .ToArray();
        }

        public int GetCount(long fromOffsetExclusive, long toOffsetInclusive)
        {
            return eventLogEntityStorage
                .GetCount(
                    e => e.Offset > fromOffsetExclusive
                         && e.Offset <= toOffsetInclusive
                         && e.EntityType == entityTypeName);
        }

        public long GetLastOffset()
        {
            return eventLogEntityStorage.GetMaxValue(
                e => e.Offset,
                filter : e => e.EntityType == entityTypeName);
        }

        public long GetLastOffsetForTimestamp(DateTime timestamp)
        {
            return eventLogEntityStorage.GetMaxValue(
                e => e.Offset,
                filter : e => e.Timestamp <= timestamp && e.EntityType == entityTypeName);
        }

        public DateTime GetMaxTimestampForOffset(long offset)
        {
            return eventLogEntityStorage.GetMaxValue(
                e => e.Timestamp,
                filter : e => e.Offset <= offset && e.EntityType == entityTypeName);
        }

        [NotNull]
        private static EntityEvent<TEntity> BuildEntityEvent([NotNull] EventLogEntity e)
        {
            var entitySnapshot = JsonConvert.DeserializeObject<TEntity>(e.EntityContent);
            return new EntityEvent<TEntity>
                {
                    EventId = e.Id,
                    EventOffset = e.Offset,
                    EventType = ParseEntityEventType(e.Type),
                    EntitySnapshot = entitySnapshot,
                };
        }

        private static EntityEventType ParseEntityEventType([CanBeNull] string type)
        {
            switch (type)
            {
            case "INSERT":
                return EntityEventType.Create;
            case "UPDATE":
                return EntityEventType.Update;
            case "DELETE":
                return EntityEventType.Delete;
            default:
                throw new InvalidProgramStateException($"Unknown entity event log event type: {type}");
            }
        }

        private readonly IEntityStorage<EventLogEntity> eventLogEntityStorage;

        [NotNull]
        private readonly string entityTypeName;
    }
}