﻿using System;
using System.Linq;
using System.Linq.Expressions;

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
                    throw new InvalidProgramStateException($"EventLog entity type name not found for {entityType.Name}");
                return name;
            }
        }

        [NotNull, ItemNotNull]
        public SqlEvent<TEntity>[] GetEvents(long? fromOffsetExclusive, int limit)
        {
            if (fromOffsetExclusive.HasValue && fromOffsetExclusive.Value < 0)
                throw new ArgumentException($"{nameof(fromOffsetExclusive)} must be non-negative number", nameof(fromOffsetExclusive));

            Expression<Func<SqlEventLogEntry, bool>> searchCriterion;
            if (fromOffsetExclusive.HasValue)
                searchCriterion = e => e.Offset > fromOffsetExclusive && e.EntityType == entityTypeName;
            else
                searchCriterion = e => e.EntityType == entityTypeName;

            return eventLogSqlStorage.Find(searchCriterion, e => e.Offset, limit).Select(BuildEntityEvent).ToArray();
        }

        public int GetEventsCount(long? fromOffsetExclusive)
        {
            using (var context = createDbContext())
            {
                Expression<Func<SqlEventLogEntry, bool>> predicate;
                if (fromOffsetExclusive.HasValue)
                    predicate = e => e.Offset > fromOffsetExclusive.Value && e.EntityType == entityTypeName;
                else
                    predicate = e => e.EntityType == entityTypeName;
                return context.Set<SqlEventLogEntry>().Count(predicate);
            }
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

        private readonly IConcurrentSqlStorage<SqlEventLogEntry, Guid> eventLogSqlStorage;
        private readonly Func<SqlDbContext> createDbContext;

        [NotNull]
        private readonly string entityTypeName;
    }
}