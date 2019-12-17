using System;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using SkbKontur.SqlStorageCore.Schema;

namespace SkbKontur.SqlStorageCore.EventLog
{
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

        private static string GetEventLogEntityTypeName(Func<SqlDbContext> createDbContext, Type entityType)
        {
            using var context = createDbContext();
            var name = context.Model.FindEntityType(entityType)?.Relational()?.TableName;
            return name ?? throw new InvalidOperationException($"EventLog entity type name not found for {entityType.Name}");
        }

        public SqlEvent<TEntity>[] GetEvents(long? fromOffsetExclusive, int limit)
        {
            ValidateOffset(fromOffsetExclusive);
            var searchCriterion = BuildEventsSearchCriterion(fromOffsetExclusive);
            return eventLogSqlStorage.Find(searchCriterion, e => e.Offset, limit).Select(BuildEntityEvent).ToArray();
        }

        public int GetEventsCount(long? fromOffsetExclusive)
        {
            ValidateOffset(fromOffsetExclusive);

            using (var context = createDbContext())
            {
                var predicate = BuildEventsSearchCriterion(fromOffsetExclusive);
                return context.Set<SqlEventLogEntry>().Count(predicate);
            }
        }

        private Expression<Func<SqlEventLogEntry, bool>> BuildEventsSearchCriterion(long? fromOffsetExclusive)
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

        private void ValidateOffset(long? offset)
        {
            if (offset == null)
                return;
            if (offset.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), offset.Value, "Must be non-negative");
        }

        private static SqlEvent<TEntity> BuildEntityEvent(SqlEventLogEntry e)
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

        private static SqlEventType ParseEntityEventType(string? eventType)
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

        private readonly string entityTypeName;
    }
}