using System;
using System.Linq;

using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.EventLog
{
    [UsedImplicitly]
    public class SqlEventLogRepository<TEntity, TKey> : ISqlEventLogRepository<TEntity, TKey>
        where TEntity : ISqlEntity<TKey>
    {
        public SqlEventLogRepository(ISqlStorage<SqlEventLogEntry, Guid> eventLogSqlStorage, Func<SqlDbContext> createDbContext)
        {
            this.eventLogSqlStorage = eventLogSqlStorage;
            var entityType = typeof(TEntity);
            entityTypeName = GetEventLogEntityTypeName(createDbContext, entityType);
        }

        [NotNull]
        private static string GetEventLogEntityTypeName([NotNull] Func<SqlDbContext> createDbContext, [NotNull] Type entityType)
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
        public SqlEvent<TEntity>[] GetEvents(long? fromOffsetExclusive, int limit)
        {
            return eventLogSqlStorage
                .Find(
                    e => e.Offset > fromOffsetExclusive && e.EntityType == entityTypeName,
                    e => e.Offset,
                    limit)
                .Select(BuildEntityEvent)
                .ToArray();
        }

        [NotNull]
        private static SqlEvent<TEntity> BuildEntityEvent([NotNull] SqlEventLogEntry e)
        {
            var entitySnapshot = JsonConvert.DeserializeObject<TEntity>(e.EntityContent);
            return new SqlEvent<TEntity>
                {
                    EventId = e.Id,
                    EventType = ParseEntityEventType(e.ModificationType),
                    EventOffset = e.Offset,
                    EventTimestamp = e.Timestamp,
                    EntitySnapshot = entitySnapshot,
                };
        }

        private static SqlEventType ParseEntityEventType([CanBeNull] string eventType)
        {
            switch (eventType)
            {
            case "INSERT":
                return SqlEventType.Create;
            case "UPDATE":
                return SqlEventType.Update;
            case "DELETE":
                return SqlEventType.Delete;
            default:
                throw new InvalidProgramStateException($"Unknown sql event type: {eventType}");
            }
        }

        private readonly ISqlStorage<SqlEventLogEntry, Guid> eventLogSqlStorage;

        [NotNull]
        private readonly string entityTypeName;
    }
}