using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using GroboContainer.NUnitExtensions;

using Microsoft.EntityFrameworkCore;

using MoreLinq;

using NUnit.Framework;

using SkbKontur.SqlStorageCore.EventLog;
using SkbKontur.SqlStorageCore.Schema;
using SkbKontur.SqlStorageCore.Tests.TestEntities;
using SkbKontur.SqlStorageCore.Tests.TestWrappers;

namespace SkbKontur.SqlStorageCore.Tests.EventLog
{
    [AndSqlStorageCleanUp(typeof(SqlEventLogEntry))]
    [TestFixture(typeof(TestValueTypedPropertiesStorageElement), typeof(Guid))]
    [TestFixture(typeof(TestJsonColumnElement), typeof(Guid))]
    [TestFixture(typeof(TestJsonArrayColumnElement), typeof(Guid))]
    public class SqlEventLogRepositoryTest<TEntity, TKey> : SqlStorageTestBase<TEntity, TKey>
        where TEntity : class, ISqlEntity<TKey>, new()
        where TKey : notnull
    {
        [GroboSetUp]
        public void SetUp()
        {
            initialOffset = GetLastOffset();
        }

        [Test]
        public async Task TestCreateSingleObject()
        {
            var entity = GenerateObjects().First();
            await sqlStorage.CreateOrUpdateAsync(entity);
            var events = await eventLogRepository.GetEventsAsync(initialOffset, 2);
            events.Length.Should().Be(1);
            var entityEvent = events.First();
            entityEvent.EntitySnapshot.Should().BeEquivalentTo(entity, equivalenceOptionsConfig);
            entityEvent.EventType.Should().Be(SqlEventType.Create);
        }

        [Test]
        public async Task TestReadWithNoStartOffset()
        {
            var entities = GenerateObjects(2).ToArray();
            await sqlStorage.CreateOrUpdateAsync(entities);
            var events = await eventLogRepository.GetEventsAsync(fromOffsetExclusive : null, int.MaxValue);
            events.Length.Should().Be(2);
            AssertUnorderedArraysEquality(events.Select(e => e.EntitySnapshot).ToArray(), entities);
        }

        [Test]
        public async Task TestCreateMultipleObjects()
        {
            var entities = GenerateObjects(testObjectsCount).ToArray();
            await sqlStorage.CreateOrUpdateAsync(entities);
            var events = await eventLogRepository.GetEventsAsync(initialOffset, entities.Length + 1);
            events.Length.Should().Be(entities.Length);
            events.All(e => e.EventType == SqlEventType.Create).Should().BeTrue();
            var actualEntities = events.Select(e => e.EntitySnapshot).ToArray();
            AssertUnorderedArraysEquality(actualEntities, entities);
        }

        [Test]
        public async Task TestUpdateSingleObject()
        {
            var entity = GenerateObjects().First();
            await sqlStorage.CreateOrUpdateAsync(entity);
            var offset = GetLastOffset();
            var updatedEntity = GenerateObjects().First();
            updatedEntity.Id = entity.Id;
            await sqlStorage.CreateOrUpdateAsync(updatedEntity);

            var events = await eventLogRepository.GetEventsAsync(offset, 2);
            events.Length.Should().Be(1);
            events.Select(e => e.EntitySnapshot).Should().AllBeEquivalentTo(updatedEntity, equivalenceOptionsConfig);
        }

        [Test]
        public async Task TestUpdateMultipleObjects()
        {
            var entities = GenerateObjects(testObjectsCount).ToArray();
            await sqlStorage.CreateOrUpdateAsync(entities);
            var offset = GetLastOffset();
            var updatedEntities = GenerateObjects(testObjectsCount).Select((e, i) =>
                {
                    e.Id = entities[i].Id;
                    return e;
                }).ToArray();

            await sqlStorage.CreateOrUpdateAsync(updatedEntities);

            var events = await eventLogRepository.GetEventsAsync(offset, entities.Length * 2 + 1);
            events.Length.Should().Be(entities.Length);
            events.All(e => e.EventType == SqlEventType.Update || e.EventType == SqlEventType.Create).Should().BeTrue();
            var snapshots = events.Select(e => e.EntitySnapshot).DistinctBy(e => e.Id).ToArray();
            snapshots.Length.Should().Be(entities.Length);
            AssertUnorderedArraysEquality(snapshots, updatedEntities);
        }

        [Test]
        public async Task TestDeleteSingleObject()
        {
            var entity = GenerateObjects().First();
            await sqlStorage.CreateOrUpdateAsync(entity);
            var offset = GetLastOffset();
            await sqlStorage.DeleteAsync(entity.Id);

            var events = await eventLogRepository.GetEventsAsync(offset, 2);

            events.Length.Should().Be(1);
            var entityEvent = events.First();
            entityEvent.EntitySnapshot.Should().BeEquivalentTo(entity, equivalenceOptionsConfig);
            entityEvent.EventType.Should().Be(SqlEventType.Delete);
        }

        [Test]
        public async Task TestDeleteMultipleObjects()
        {
            var entities = GenerateObjects(testObjectsCount).ToArray();
            await sqlStorage.CreateOrUpdateAsync(entities);
            var offset = GetLastOffset();
            await sqlStorage.DeleteAsync(entities.Select(e => e.Id).ToArray());

            var events = await eventLogRepository.GetEventsAsync(offset, entities.Length + 1);
            events.Length.Should().Be(entities.Length);
            events.All(e => e.EventType == SqlEventType.Delete).Should().BeTrue();
            AssertUnorderedArraysEquality(events.Select(e => e.EntitySnapshot), entities);
        }

        [Test]
        public async Task TestCreateAndUpdateAndDeleteMultipleObjectsThroughMultipleThreads()
        {
            var random = new Random(DateTime.Now.Millisecond);
            var initialEntities = GenerateObjects(testObjectsCount).ToArray();
            await sqlStorage.CreateOrUpdateAsync(initialEntities);
            var entitiesState = new ConcurrentDictionary<TKey, SqlEventType>();
            var offset = GetLastOffset();
            var eventsCount = 0;
            Parallel.ForEach(initialEntities.Batch(testObjectsCount / 10), batch =>
                {
                    foreach (var entity in batch)
                    {
                        var operation = random.Next(4);
                        switch (operation)
                        {
                        case 0:
                            var updated = GenerateObjects().First();
                            updated.Id = entity.Id;
                            sqlStorage.CreateOrUpdateAsync(updated).GetAwaiter().GetResult();
                            eventsCount += 2;
                            entitiesState.TryAdd(entity.Id, SqlEventType.Update);
                            continue;
                        case 1:
                            sqlStorage.DeleteAsync(entity.Id).GetAwaiter().GetResult();
                            eventsCount++;
                            entitiesState.TryAdd(entity.Id, SqlEventType.Delete);
                            continue;
                        case 2:
                            var newEntity = GenerateObjects().First();
                            sqlStorage.CreateOrUpdateAsync(newEntity).GetAwaiter().GetResult();
                            eventsCount++;
                            entitiesState.TryAdd(newEntity.Id, SqlEventType.Create);
                            continue;
                        default:
                            continue;
                        }
                    }
                });

            var events = await eventLogRepository.GetEventsAsync(offset, testObjectsCount + eventsCount);
            var lastEvents = events.GroupBy(e => e.EntitySnapshot.Id).SelectMany(g => g.MaxBy(@event => @event.EventOffset)).ToArray();
            lastEvents.Select(e => e.EntitySnapshot.Id).Should().BeEquivalentTo(entitiesState.Select(e => e.Key));
            foreach (var entityEvent in lastEvents)
            {
                entityEvent.EventType.Should().Be(entitiesState[entityEvent.EntitySnapshot.Id]);
            }
        }

        [Test]
        public async Task TestLimit()
        {
            var expectedEntities = GenerateObjects(testObjectsCount).ToArray();
            await sqlStorage.CreateOrUpdateAsync(expectedEntities);
            var notExpectedEntities = GenerateObjects(testObjectsCount).ToArray();
            await sqlStorage.CreateOrUpdateAsync(notExpectedEntities);
            var events = await eventLogRepository.GetEventsAsync(initialOffset, expectedEntities.Length);
            events.Length.Should().Be(expectedEntities.Length);
            var actualSnapshots = events.Select(e => e.EntitySnapshot).ToArray();
            AssertUnorderedArraysEquality(actualSnapshots, expectedEntities);
        }

        [Test]
        public async Task TestOffsetOrdering()
        {
            var entity = GenerateObjects().First();
            await sqlStorage.CreateOrUpdateAsync(entity);
            await sqlStorage.CreateOrUpdateAsync(entity);
            await sqlStorage.DeleteAsync(entity.Id);
            var events = await eventLogRepository.GetEventsAsync(initialOffset, 3);
            events.Should().BeInAscendingOrder(e => e.EventOffset);
            events.Select(e => e.EventType)
                  .Should()
                  .BeEquivalentTo(
                      new[]
                          {
                              SqlEventType.Create,
                              SqlEventType.Update,
                              SqlEventType.Delete
                          },
                      options => options.WithStrictOrdering());
        }

        [Test]
        public async Task TestGetEventsCount()
        {
            const int entitiesCount = 5;
            var entities = GenerateObjects(entitiesCount).ToArray();
            (await eventLogRepository.GetEventsCountAsync(fromOffsetExclusive : null)).Should().Be(0);

            await sqlStorage.CreateOrUpdateAsync(entities);
            var events = await eventLogRepository.GetEventsAsync(null, entitiesCount);

            (await eventLogRepository.GetEventsCountAsync(fromOffsetExclusive : null)).Should().Be(entitiesCount);
            (await eventLogRepository.GetEventsCountAsync(fromOffsetExclusive : events[1].EventOffset)).Should().Be(3);
            var lastEventOffset = events.Last().EventOffset;
            var farFutureOffset = lastEventOffset + 1000;
            (await eventLogRepository.GetEventsCountAsync(fromOffsetExclusive : farFutureOffset)).Should().Be(0);
        }

        [TestCaseSource(nameof(testParallelTransactionsOrderingSource))]
        public async Task TestOffsetOrderingParallelTransactions(IsolationLevel firstTransactionIsolationLevel, IsolationLevel secondTransactionIsolationLevel, bool withPrecedingEvents)
        {
            if (withPrecedingEvents) await GeneratePrecedingEvents();

            var fromOffsetExclusive = GetLastOffset();
            var entities = GenerateObjects(2).ToArray();
            var firstTransactionReady = new AutoResetEvent(false);
            var secondTransactionFinish = new AutoResetEvent(false);

            var firstTransaction = sqlStorage.BatchAsync(async storage =>
                {
                    await storage.CreateOrUpdateAsync<TEntity, TKey>(entities[0]);
                    firstTransactionReady.Set();
                    secondTransactionFinish.WaitOne();
                    await storage.CreateOrUpdateAsync<TEntity, TKey>(entities[0]);
                }, firstTransactionIsolationLevel).ConfigureAwait(false);

            firstTransactionReady.WaitOne();
            await sqlStorage.BatchAsync(async storage => await storage.CreateOrUpdateAsync<TEntity, TKey>(entities[1]), secondTransactionIsolationLevel);
            var secondTransactionEvents = await eventLogRepository.GetEventsAsync(fromOffsetExclusive, 1);
            secondTransactionEvents.Should().BeEmpty();

            secondTransactionFinish.Set();
            await firstTransaction;

            var events = await eventLogRepository.GetEventsAsync(fromOffsetExclusive, 4);
            events.Length.Should().Be(3);
            events.Should().BeInAscendingOrder(e => e.EventOffset);
        }

        [TestCaseSource(nameof(testParallelTransactionsCountSource))]
        public async Task TestGetEventsCountParallelTransactions(IsolationLevel firstTransactionIsolationLevel, IsolationLevel secondTransactionIsolationLevel, bool withPrecedingEvents)
        {
            if (withPrecedingEvents) await GeneratePrecedingEvents();

            var fromOffsetExclusive = GetLastOffset();
            var entities = GenerateObjects(2).ToArray();
            var firstTransactionReady = new AutoResetEvent(false);
            var secondTransactionFinish = new AutoResetEvent(false);

            var firstTransaction = sqlStorage.BatchAsync(async storage =>
                {
                    await storage.CreateOrUpdateAsync<TEntity, TKey>(entities[0]);
                    firstTransactionReady.Set();
                    secondTransactionFinish.WaitOne();
                    await storage.CreateOrUpdateAsync<TEntity, TKey>(entities[0]);
                }, firstTransactionIsolationLevel).ConfigureAwait(false);

            firstTransactionReady.WaitOne();
            await sqlStorage.BatchAsync(async storage => await storage.CreateOrUpdateAsync<TEntity, TKey>(entities[1]), secondTransactionIsolationLevel);
            (await eventLogRepository.GetEventsCountAsync(fromOffsetExclusive)).Should().Be(0);

            secondTransactionFinish.Set();
            await firstTransaction;

            (await eventLogRepository.GetEventsCountAsync(fromOffsetExclusive)).Should().Be(3);
        }

        private async Task GeneratePrecedingEvents()
        {
            var preceding = GenerateObjects(100).ToArray();
            await sqlStorage.CreateOrUpdateAsync(preceding);
        }

        private long? GetLastOffset()
        {
            using var context = dbContextCreator.Create();
            var entityTypeName = context.Model.FindEntityType(typeof(TEntity))?.GetTableName();
            Expression<Func<SqlEventLogEntry, bool>> filter = e => e.EntityType == entityTypeName
                                                                   && e.TransactionId < PostgresFunctions.SnapshotMinimalTransactionId(PostgresFunctions.CurrentTransactionIdsSnapshot());
            if (!context.Set<SqlEventLogEntry>().Any(filter))
                return null;
            return context.Set<SqlEventLogEntry>().Where(filter).Max(e => e.Offset);
        }

        private const int testObjectsCount = 100;

        private long? initialOffset;

        [Injected]
        private readonly ISqlEventLogRepository<TEntity, TKey> eventLogRepository;

        [Injected]
        private readonly DbContextCreator dbContextCreator;

        private class DbContextCreator
        {
            public DbContextCreator(Func<SqlDbContext> createDbContext)
            {
                this.createDbContext = createDbContext;
            }

            public SqlDbContext Create()
            {
                return createDbContext();
            }

            private readonly Func<SqlDbContext> createDbContext;
        }

        // ReSharper disable StaticMemberInGenericType
        private static readonly IsolationLevel[] supportedIsolationLevels =
            {
                IsolationLevel.ReadCommitted,
                IsolationLevel.ReadUncommitted,
                IsolationLevel.RepeatableRead,
                IsolationLevel.Serializable,
                IsolationLevel.Snapshot,
                IsolationLevel.Unspecified,
            };

        private static readonly IsolationLevel[][] supportedTransactionIsolationLevelsCartesian = supportedIsolationLevels
                                                                                                  .SelectMany(first => supportedIsolationLevels.Select(second => new[] {first, second}))
                                                                                                  .ToArray();

        private static readonly object[][] testParallelTransactionsOrderingSource = new[] {true, false}
                                                                                    .SelectMany(withPrecedingEvents => supportedTransactionIsolationLevelsCartesian.Select(levels => new object[] {levels[0], levels[1], withPrecedingEvents}))
                                                                                    .ToArray();

        private static readonly object[][] testParallelTransactionsCountSource = new[] {true, false}
                                                                                 .SelectMany(withPrecedingEvents => supportedTransactionIsolationLevelsCartesian.Select(levels => new object[] {levels[0], levels[1], withPrecedingEvents}))
                                                                                 .ToArray();

        // ReSharper restore StaticMemberInGenericType
    }
}