using System;
using System.Linq;
using System.Linq.Expressions;

using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using SkbKontur.SqlStorageCore.Schema;

namespace SkbKontur.SqlStorageCore.EventLog
{
    [UsedImplicitly]
    public class SqlEventLogRepository<TEntity, TKey> : ISqlEventLogRepository<TEntity, TKey>
        where TEntity : ISqlEntity<TKey>
    {
        public SqlEventLogRepository(IConcurrentSqlStorage<SqlEventLogEntry, Guid> eventLogSqlStorage, Func<SqlDbContext> createDbContext)
        {
            this.eventLogSqlStorage = eventLogSqlStorage;
            this.createDbContext = createDbContext;
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
                    throw new InvalidOperationException($"EventLog entity type name not found for {entityType.Name}");
                return name;
            }
        }

        [NotNull, ItemNotNull]
        public SqlEvent<TEntity>[] GetEvents([CanBeNull] long? fromOffsetExclusive, int limit)
        {
            ValidateOffset(fromOffsetExclusive);
            var searchCriterion = BuildEventsSearchCriterion(fromOffsetExclusive);
            return eventLogSqlStorage.Find(searchCriterion, e => e.Offset, limit).Select(BuildEntityEvent).ToArray();
        }

        public int GetEventsCount([CanBeNull] long? fromOffsetExclusive)
        {
            ValidateOffset(fromOffsetExclusive);

            using (var context = createDbContext())
            {
                var predicate = BuildEventsSearchCriterion(fromOffsetExclusive);
                return context.Set<SqlEventLogEntry>().Count(predicate);
            }
        }

        [NotNull]
        private Expression<Func<SqlEventLogEntry, bool>> BuildEventsSearchCriterion([CanBeNull] long? fromOffsetExclusive)
        {
            Expression<Func<SqlEventLogEntry, bool>> searchCriterion;
            if (fromOffsetExclusive.HasValue)
                searchCriterion = e => e.Offset > fromOffsetExclusive.Value
                                       && e.EntityType == entityTypeName
                                       && e.TransactionId < PostgresFunctions.SnapshotMinimalTransactionId(PostgresFunctions.CurrentTransactionIdsSnapshot());
            else
                searchCriterion = e => e.EntityType == entityTypeName
                                       && e.TransactionId < PostgresFunctions.SnapshotMinimalTransactionId(PostgresFunctions.CurrentTransactionIdsSnapshot());
            return searchCriterion;
        }

        private void ValidateOffset([CanBeNull] long? offset)
        {
            if (offset == null)
                return;
            if (offset.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), offset.Value, "Must be non-negative");
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
                throw new ArgumentOutOfRangeException($"Unknown sql event type: {eventType}");
            }
        }

        private readonly IConcurrentSqlStorage<SqlEventLogEntry, Guid> eventLogSqlStorage;
        private readonly Func<SqlDbContext> createDbContext;

        [NotNull]
        private readonly string entityTypeName;
    }
}