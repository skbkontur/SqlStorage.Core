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
    public class SqlEventLogRepository<TEntity> : ISqlEventLogRepository<TEntity>
        where TEntity : IIdentifiableSqlEntity
    {
        public SqlEventLogRepository(ISqlStorage<EventLogStorageElement> eventLogSqlStorage, Func<SqlDatabaseContext> createDbContext)
        {
            this.eventLogSqlStorage = eventLogSqlStorage;
            var entityType = typeof(TEntity);
            entityTypeName = GetEventLogEntityTypeName(createDbContext, entityType);
        }

        [NotNull]
        private static string GetEventLogEntityTypeName([NotNull] Func<SqlDatabaseContext> createDbContext, [NotNull] Type entityType)
        {
            using (var context = createDbContext())
            {
                var name = context.Model.FindEntityType(entityType)?.Relational()?.TableName;
                if (string.IsNullOrEmpty(name))
                    throw new InvalidProgramStateException($"EventLog entity type name not found for {entityType.Name}");
                return name;
            }
        }

        [NotNull, ItemNotNull]
        public SqlEvent<TEntity>[] GetEvents(long fromOffsetExclusive, long toOffsetInclusive, int limit)
        {
            return eventLogSqlStorage
                .Find(
                    e => e.Offset > fromOffsetExclusive
                         && e.Offset <= toOffsetInclusive
                         && e.EntityType == entityTypeName,
                    e => e.Offset,
                    limit)
                .Select(BuildEntityEvent)
                .ToArray();
        }

        public int GetCount(long fromOffsetExclusive, long toOffsetInclusive)
        {
            return eventLogSqlStorage
                .GetCount(
                    e => e.Offset > fromOffsetExclusive
                         && e.Offset <= toOffsetInclusive
                         && e.EntityType == entityTypeName);
        }

        public long GetLastOffset()
        {
            return eventLogSqlStorage.GetMaxValue(
                e => e.Offset,
                filter : e => e.EntityType == entityTypeName);
        }

        public long GetLastOffsetForTimestamp(DateTime timestamp)
        {
            return eventLogSqlStorage.GetMaxValue(
                e => e.Offset,
                filter : e => e.Timestamp <= timestamp && e.EntityType == entityTypeName);
        }

        public DateTime GetMaxTimestampForOffset(long offset)
        {
            return eventLogSqlStorage.GetMaxValue(
                e => e.Timestamp,
                filter : e => e.Offset <= offset && e.EntityType == entityTypeName);
        }

        [NotNull]
        private static SqlEvent<TEntity> BuildEntityEvent([NotNull] EventLogStorageElement e)
        {
            var entitySnapshot = JsonConvert.DeserializeObject<TEntity>(e.EntityContent);
            return new SqlEvent<TEntity>
                {
                    EventId = e.Id,
                    EventOffset = e.Offset,
                    EventType = ParseEntityEventType(e.Type),
                    EntitySnapshot = entitySnapshot,
                };
        }

        private static SqlEventType ParseEntityEventType([CanBeNull] string type)
        {
            switch (type)
            {
            case "INSERT":
                return SqlEventType.Create;
            case "UPDATE":
                return SqlEventType.Update;
            case "DELETE":
                return SqlEventType.Delete;
            default:
                throw new InvalidProgramStateException($"Unknown sql eventLog event type: {type}");
            }
        }

        private readonly ISqlStorage<EventLogStorageElement> eventLogSqlStorage;

        [NotNull]
        private readonly string entityTypeName;
    }
}