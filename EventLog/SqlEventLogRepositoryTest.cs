using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using GroboContainer.NUnitExtensions;

using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;

using MoreLinq;

using NUnit.Framework;

using SKBKontur.Catalogue.EDI.SqlStorageCore;
using SKBKontur.Catalogue.EDI.SqlStorageCore.EventLog;
using SKBKontur.Catalogue.EDI.SqlStorageCore.Schema;
using SKBKontur.Catalogue.EDIFunctionalTests.Commons.TestWrappers;
using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EventLog
{
    [AndSqlStorageCleanUp(typeof(SqlEventLogEntry))]
    [TestFixture(typeof(TestValueTypedPropertiesStorageElement), typeof(Guid))]
    [TestFixture(typeof(TestJsonColumnElement), typeof(Guid))]
    [TestFixture(typeof(TestJsonArrayColumnElement), typeof(Guid))]
    public class SqlEventLogRepositoryTest<TEntity, TKey> : SqlStorageTestBase<TEntity, TKey>
        where TEntity : class, ISqlEntity<TKey>, new()
    {
        [GroboSetUp]
        public void SetUp()
        {
            initialOffset = GetLastOffset();
        }

        [Test]
        public void TestCreateSingleObject()
        {
            var entity = GenerateObjects().First();
            sqlStorage.CreateOrUpdate(entity);
            var events = eventLogRepository.GetEvents(initialOffset, 2);
            events.Length.Should().Be(1);
            var entityEvent = events.First();
            entityEvent.EntitySnapshot.Should().BeEquivalentTo(entity, equivalenceOptionsConfig);
            entityEvent.EventType.Should().Be(SqlEventType.Create);
        }

        [Test]
        public void TestReadWithNoStartOffset()
        {
            var entities = GenerateObjects(2).ToArray();
            sqlStorage.CreateOrUpdate(entities);
            var events = eventLogRepository.GetEvents(fromOffsetExclusive : null, int.MaxValue);
            events.Length.Should().Be(2);
            AssertUnorderedArraysEquality(events.Select(e => e.EntitySnapshot).ToArray(), entities);
        }

        [Test]
        public void TestCreateMultipleObjects()
        {
            var entities = GenerateObjects(testObjectsCount).ToArray();
            sqlStorage.CreateOrUpdate(entities);
            var events = eventLogRepository.GetEvents(initialOffset, entities.Length + 1);
            events.Length.Should().Be(entities.Length);
            events.All(e => e.EventType == SqlEventType.Create).Should().BeTrue();
            var actualEntities = events.Select(e => e.EntitySnapshot).ToArray();
            AssertUnorderedArraysEquality(actualEntities, entities);
        }

        [Test]
        public void TestUpdateSingleObject()
        {
            var entity = GenerateObjects().First();
            sqlStorage.CreateOrUpdate(entity);
            var offset = GetLastOffset();
            var updatedEntity = GenerateObjects().First();
            updatedEntity.Id = entity.Id;
            sqlStorage.CreateOrUpdate(updatedEntity);

            var events = eventLogRepository.GetEvents(offset, 2);
            events.Length.Should().Be(1);
            events.Select(e => e.EntitySnapshot).Should().AllBeEquivalentTo(updatedEntity, equivalenceOptionsConfig);
        }

        [Test]
        public void TestUpdateMultipleObjects()
        {
            var entities = GenerateObjects(testObjectsCount).ToArray();
            sqlStorage.CreateOrUpdate(entities);
            var offset = GetLastOffset();
            var updatedEntities = GenerateObjects(testObjectsCount).Select((e, i) =>
                {
                    e.Id = entities[i].Id;
                    return e;
                }).ToArray();

            sqlStorage.CreateOrUpdate(updatedEntities);

            var events = eventLogRepository.GetEvents(offset, entities.Length * 2 + 1);
            events.Length.Should().Be(entities.Length);
            events.All(e => e.EventType == SqlEventType.Update || e.EventType == SqlEventType.Create).Should().BeTrue();
            var snapshots = events.Select(e => e.EntitySnapshot).DistinctBy(e => e.Id).ToArray();
            snapshots.Length.Should().Be(entities.Length);
            AssertUnorderedArraysEquality(snapshots, updatedEntities);
        }

        [Test]
        public void TestDeleteSingleObject()
        {
            var entity = GenerateObjects().First();
            sqlStorage.CreateOrUpdate(entity);
            var offset = GetLastOffset();
            sqlStorage.Delete(entity.Id);

            var events = eventLogRepository.GetEvents(offset, 2);

            events.Length.Should().Be(1);
            var entityEvent = events.First();
            entityEvent.EntitySnapshot.Should().BeEquivalentTo(entity, equivalenceOptionsConfig);
            entityEvent.EventType.Should().Be(SqlEventType.Delete);
        }

        [Test]
        public void TestDeleteMultipleObjects()
        {
            var entities = GenerateObjects(testObjectsCount).ToArray();
            sqlStorage.CreateOrUpdate(entities);
            var offset = GetLastOffset();
            sqlStorage.Delete(entities.Select(e => e.Id).ToArray());

            var events = eventLogRepository.GetEvents(offset, entities.Length + 1);
            events.Length.Should().Be(entities.Length);
            events.All(e => e.EventType == SqlEventType.Delete).Should().BeTrue();
            AssertUnorderedArraysEquality(events.Select(e => e.EntitySnapshot), entities);
        }

        [Test]
        public void TestCreateAndUpdateAndDeleteMultipleObjectsThroughMultipleThreads()
        {
            var random = new Random(DateTime.Now.Millisecond);
            var initialEntities = GenerateObjects(testObjectsCount).ToArray();
            sqlStorage.CreateOrUpdate(initialEntities);
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
                            sqlStorage.CreateOrUpdate(updated);
                            eventsCount += 2;
                            entitiesState.TryAdd(entity.Id, SqlEventType.Update);
                            continue;
                        case 1:
                            sqlStorage.Delete(entity.Id);
                            eventsCount++;
                            entitiesState.TryAdd(entity.Id, SqlEventType.Delete);
                            continue;
                        case 2:
                            var newEntity = GenerateObjects().First();
                            sqlStorage.CreateOrUpdate(newEntity);
                            eventsCount++;
                            entitiesState.TryAdd(newEntity.Id, SqlEventType.Create);
                            continue;
                        default:
                            continue;
                        }
                    }
                });

            var events = eventLogRepository.GetEvents(offset, testObjectsCount + eventsCount);
            var lastEvents = events.GroupBy(e => e.EntitySnapshot.Id).SelectMany(g => g.MaxBy(@event => @event.EventOffset)).ToArray();
            lastEvents.Select(e => e.EntitySnapshot.Id).Should().BeEquivalentTo(entitiesState.Select(e => e.Key));
            foreach (var entityEvent in lastEvents)
            {
                entityEvent.EventType.Should().Be(entitiesState[entityEvent.EntitySnapshot.Id]);
            }
        }

        [Test]
        public void TestLimit()
        {
            var expectedEntities = GenerateObjects(testObjectsCount).ToArray();
            sqlStorage.CreateOrUpdate(expectedEntities);
            var notExpectedEntities = GenerateObjects(testObjectsCount).ToArray();
            sqlStorage.CreateOrUpdate(notExpectedEntities);
            var events = eventLogRepository.GetEvents(initialOffset, expectedEntities.Length);
            events.Length.Should().Be(expectedEntities.Length);
            var actualSnapshots = events.Select(e => e.EntitySnapshot).ToArray();
            AssertUnorderedArraysEquality(actualSnapshots, expectedEntities);
        }

        [Test]
        public void TestOffsetOrdering()
        {
            var entity = GenerateObjects().First();
            sqlStorage.CreateOrUpdate(entity);
            sqlStorage.CreateOrUpdate(entity);
            sqlStorage.Delete(entity.Id);
            var events = eventLogRepository.GetEvents(initialOffset, 3);
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
        public void TestGetEventsCount()
        {
            const int entitiesCount = 5;
            var entities = GenerateObjects(entitiesCount).ToArray();
            eventLogRepository.GetEventsCount(fromOffsetExclusive : null).Should().Be(0);

            sqlStorage.CreateOrUpdate(entities);
            var events = eventLogRepository.GetEvents(null, entitiesCount);

            eventLogRepository.GetEventsCount(fromOffsetExclusive : null).Should().Be(entitiesCount);
            eventLogRepository.GetEventsCount(fromOffsetExclusive : events[1].EventOffset).Should().Be(3);
            var lastEventOffset = events.Last().EventOffset;
            var farFutureOffset = lastEventOffset + 1000;
            eventLogRepository.GetEventsCount(fromOffsetExclusive : farFutureOffset).Should().Be(0);
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

        // ReSharper restore StaticMemberInGenericType

        // ReSharper disable once StaticMemberInGenericType
        private static readonly object[][] testParallelTransactionsOrderingSource = new[] {true, false}
            .SelectMany(withPrecedingEvents => supportedTransactionIsolationLevelsCartesian.Select(levels => new object[] {levels[0], levels[1], withPrecedingEvents}))
            .ToArray();

        [TestCaseSource(nameof(testParallelTransactionsOrderingSource))]
        public async Task TestOffsetOrderingParallelTransactions(IsolationLevel firstTransactionIsolationLevel, IsolationLevel secondTransactionIsolationLevel, bool withPrecedingEvents)
        {
            if (withPrecedingEvents) GeneratePrecedingEvents();

            var fromOffsetExclusive = GetLastOffset();
            var entities = GenerateObjects(2).ToArray();
            var firstTransactionReady = new AutoResetEvent(false);
            var secondTransactionFinish = new AutoResetEvent(false);

            var firstTransaction = Task.Run(() => sqlStorage.Batch(storage =>
                {
                    storage.CreateOrUpdate<TEntity, TKey>(entities[0]);
                    firstTransactionReady.Set();
                    secondTransactionFinish.WaitOne();
                    storage.CreateOrUpdate<TEntity, TKey>(entities[0]);
                }, firstTransactionIsolationLevel)).ConfigureAwait(false);

            firstTransactionReady.WaitOne();
            sqlStorage.Batch(storage => storage.CreateOrUpdate<TEntity, TKey>(entities[1]), secondTransactionIsolationLevel);
            var secondTransactionEvents = eventLogRepository.GetEvents(fromOffsetExclusive, 1);
            secondTransactionEvents.Should().BeEmpty();

            secondTransactionFinish.Set();
            await firstTransaction;

            var events = eventLogRepository.GetEvents(fromOffsetExclusive, 4);
            events.Length.Should().Be(3);
            events.Should().BeInAscendingOrder(e => e.EventOffset);
        }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly object[][] testParallelTransactionsCountSource = new[] {true, false}
            .SelectMany(withPrecedingEvents => supportedTransactionIsolationLevelsCartesian.Select(levels => new object[] {levels[0], levels[1], withPrecedingEvents}))
            .ToArray();

        [TestCaseSource(nameof(testParallelTransactionsCountSource))]
        public async Task TestGetEventsCountParallelTransactions(IsolationLevel firstTransactionIsolationLevel, IsolationLevel secondTransactionIsolationLevel, bool withPrecedingEvents)
        {
            if (withPrecedingEvents) GeneratePrecedingEvents();

            var fromOffsetExclusive = GetLastOffset();
            var entities = GenerateObjects(2).ToArray();
            var firstTransactionReady = new AutoResetEvent(false);
            var secondTransactionFinish = new AutoResetEvent(false);

            var firstTransaction = Task.Run(() => sqlStorage.Batch(storage =>
                {
                    storage.CreateOrUpdate<TEntity, TKey>(entities[0]);
                    firstTransactionReady.Set();
                    secondTransactionFinish.WaitOne();
                    storage.CreateOrUpdate<TEntity, TKey>(entities[0]);
                }, firstTransactionIsolationLevel)).ConfigureAwait(false);

            firstTransactionReady.WaitOne();
            sqlStorage.Batch(storage => storage.CreateOrUpdate<TEntity, TKey>(entities[1]), secondTransactionIsolationLevel);
            eventLogRepository.GetEventsCount(fromOffsetExclusive).Should().Be(0);

            secondTransactionFinish.Set();
            await firstTransaction;

            eventLogRepository.GetEventsCount(fromOffsetExclusive).Should().Be(3);
        }

        private void GeneratePrecedingEvents()
        {
            var preceding = GenerateObjects(100).ToArray();
            sqlStorage.CreateOrUpdate(preceding);
        }

        private long? GetLastOffset()
        {
            using (var context = dbContextCreator.Create())
            {
                var entityTypeName = context.Model.FindEntityType(typeof(TEntity))?.Relational()?.TableName;
                Expression<Func<SqlEventLogEntry, bool>> filter = e => e.EntityType == entityTypeName
                                                                       && e.TransactionId < PostgresFunctions.SnapshotMinimalTransactionId(PostgresFunctions.CurrentTransactionIdsSnapshot());
                if (!context.Set<SqlEventLogEntry>().Any(filter))
                    return null;
                return context.Set<SqlEventLogEntry>().Where(filter).Max(e => e.Offset);
            }
        }

        private long? initialOffset;

        private const int testObjectsCount = 100;

        [Injected]
        private readonly ISqlEventLogRepository<TEntity, TKey> eventLogRepository;

        [Injected]
        private readonly DbContextCreator dbContextCreator;

        [UsedImplicitly]
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
    }
}