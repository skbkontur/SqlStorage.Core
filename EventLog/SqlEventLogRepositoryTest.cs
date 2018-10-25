using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using MoreLinq;

using NUnit.Framework;

using SKBKontur.Catalogue.EDI.SqlStorageCore;
using SKBKontur.Catalogue.EDI.SqlStorageCore.EventLog;
using SKBKontur.Catalogue.EDIFunctionalTests.Commons.TestWrappers;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
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
        [EdiSetUp]
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

        [Test] // todo (iperevoschikov, 18.09.2018): INSERT .. ON CONFLICT UPDATE ... produces two event log records when conflict occurs (trigger fires for insert and for update)
        public void TestUpdateSingleObject()
        {
            var entity = GenerateObjects().First();
            sqlStorage.CreateOrUpdate(entity);
            var offset = GetLastOffset();
            var updatedEntity = GenerateObjects().First();
            updatedEntity.Id = entity.Id;
            sqlStorage.CreateOrUpdate(updatedEntity);

            var events = eventLogRepository.GetEvents(offset, 3);
            events.Length.Should().Be(2);
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
            events.Length.Should().Be(entities.Length * 2);
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
        public void TestOrdering()
        {
            var entity = GenerateObjects().First();
            var offset = GetLastOffset();
            sqlStorage.CreateOrUpdate(entity);
            sqlStorage.CreateOrUpdate(entity);
            sqlStorage.Delete(entity.Id);
            var events = eventLogRepository.GetEvents(offset, 4);
            events.Should().BeInAscendingOrder(e => e.EventOffset);
            events.Select(e => e.EventType)
                  .Should()
                  .BeEquivalentTo(
                      new[]
                          {
                              SqlEventType.Create,
                              SqlEventType.Create,
                              SqlEventType.Update,
                              SqlEventType.Delete
                          },
                      options => options.WithStrictOrdering());
        }

        private long GetLastOffset()
        {
            using (var context = dbContextCreator.Create())
            {
                var entityTypeName = context.Model.FindEntityType(typeof(TEntity))?.Relational()?.TableName;
                Expression<Func<SqlEventLogEntry, bool>> filter = e => e.EntityType == entityTypeName;
                if (!context.Set<SqlEventLogEntry>().Any(filter))
                    return default;
                return context.Set<SqlEventLogEntry>().Where(filter).Max(e => e.Offset);
            }
        }

        private long initialOffset;

        private const int testObjectsCount = 100;

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
    }
}