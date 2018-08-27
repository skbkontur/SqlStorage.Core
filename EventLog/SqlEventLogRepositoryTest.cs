using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MoreLinq;

using NUnit.Framework;

using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;
using SKBKontur.Catalogue.EDI.SqlStorageCore.EventLog;
using SKBKontur.Catalogue.EDIFunctionalTests.Commons.TestWrappers;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EventLog
{
    [TestFixture(typeof(TestValueTypedPropertiesStorageElement))]
    [TestFixture(typeof(TestJsonColumnElement))]
    [TestFixture(typeof(TestJsonArrayColumnElement))]
    public class SqlEventLogRepositoryTest<TEntity> : SqlStorageTestBase<TEntity>
        where TEntity : class, IIdentifiableSqlEntity, new()
    {
        [EdiSetUp]
        public void SetUp()
        {
            initialOffset = eventLogRepository.GetLastOffset();
        }

        [Test]
        public void TestCreateSingleObject()
        {
            var entity = GenerateObjects().First();
            sqlStorage.Create(entity);
            var events = eventLogRepository.GetEvents(initialOffset, eventLogRepository.GetLastOffset(), 2);
            events.Length.Should().Be(1);
            var entityEvent = events.First();
            entityEvent.EntitySnapshot.Should().BeEquivalentTo(entity, equivalenceOptionsConfig);
            entityEvent.EventType.Should().Be(SqlEventType.Create);
        }

        [Test]
        public void TestCreateMultipleObjects()
        {
            var entities = GenerateObjects(testObjectsCount).ToArray();
            sqlStorage.Create(entities);
            var events = eventLogRepository.GetEvents(initialOffset, eventLogRepository.GetLastOffset(), entities.Length + 1);
            events.Length.Should().Be(entities.Length);
            events.All(e => e.EventType == SqlEventType.Create).Should().BeTrue();
            var actualEntities = events.Select(e => e.EntitySnapshot).ToArray();
            AssertUnorderedArraysEquality(actualEntities, entities);
        }

        [Test]
        public void TestUpdateSingleObject()
        {
            var entity = GenerateObjects().First();
            sqlStorage.Create(entity);
            var offset = eventLogRepository.GetLastOffset();
            var updatedEntity = GenerateObjects().First();
            updatedEntity.Id = entity.Id;
            sqlStorage.Update(updatedEntity);

            var events = eventLogRepository.GetEvents(offset, eventLogRepository.GetLastOffset(), 2);
            events.Length.Should().Be(1);
            var entityEvent = events.First();
            entityEvent.EntitySnapshot.Should().BeEquivalentTo(updatedEntity, equivalenceOptionsConfig);
            entityEvent.EventType.Should().Be(SqlEventType.Update);
        }

        [Test]
        public void TestUpdateMultipleObjects()
        {
            var entities = GenerateObjects(testObjectsCount).ToArray();
            sqlStorage.Create(entities);
            var offset = eventLogRepository.GetLastOffset();
            var updatedEntities = GenerateObjects(testObjectsCount).Select((e, i) =>
                {
                    e.Id = entities[i].Id;
                    return e;
                }).ToArray();

            sqlStorage.Update(updatedEntities);

            var events = eventLogRepository.GetEvents(offset, eventLogRepository.GetLastOffset(), entities.Length + 1);
            events.Length.Should().Be(entities.Length);
            events.All(e => e.EventType == SqlEventType.Update).Should().BeTrue();
            AssertUnorderedArraysEquality(events.Select(e => e.EntitySnapshot), updatedEntities);
        }

        [Test]
        public void TestDeleteSingleObject()
        {
            var entity = GenerateObjects().First();
            sqlStorage.Create(entity);
            var offset = eventLogRepository.GetLastOffset();
            sqlStorage.Delete(entity.Id);

            var events = eventLogRepository.GetEvents(offset, eventLogRepository.GetLastOffset(), 2);

            events.Length.Should().Be(1);
            var entityEvent = events.First();
            entityEvent.EntitySnapshot.Should().BeEquivalentTo(entity, equivalenceOptionsConfig);
            entityEvent.EventType.Should().Be(SqlEventType.Delete);
        }

        [Test]
        public void TestDeleteMultipleObjects()
        {
            var entities = GenerateObjects(testObjectsCount).ToArray();
            sqlStorage.Create(entities);
            var offset = eventLogRepository.GetLastOffset();
            sqlStorage.Delete(entities.Select(e => e.Id).ToArray());

            var events = eventLogRepository.GetEvents(offset, eventLogRepository.GetLastOffset(), entities.Length + 1);
            events.Length.Should().Be(entities.Length);
            events.All(e => e.EventType == SqlEventType.Delete).Should().BeTrue();
            AssertUnorderedArraysEquality(events.Select(e => e.EntitySnapshot), entities);
        }

        [Test]
        public void TestCreateAndUpdateAndDeleteMultipleObjectsThroughMultipleThreads()
        {
            var random = new Random(DateTime.Now.Millisecond);
            var initialEntities = GenerateObjects(testObjectsCount).ToArray();
            sqlStorage.Create(initialEntities);
            var entitiesState = new ConcurrentDictionary<Guid, SqlEventType>();
            var offset = eventLogRepository.GetLastOffset();

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
                            sqlStorage.Update(updated);
                            entitiesState.TryAdd(entity.Id, SqlEventType.Update);
                            continue;
                        case 1:
                            sqlStorage.Delete(entity.Id);
                            entitiesState.TryAdd(entity.Id, SqlEventType.Delete);
                            continue;
                        case 2:
                            var newEntity = GenerateObjects().First();
                            sqlStorage.Create(newEntity);
                            entitiesState.TryAdd(newEntity.Id, SqlEventType.Create);
                            continue;
                        default:
                            continue;
                        }
                    }
                });

            var events = eventLogRepository.GetEvents(offset, eventLogRepository.GetLastOffset(), testObjectsCount * 2);
            events.Length.Should().Be(entitiesState.Count);
            events.Select(e => e.EntitySnapshot.Id).Should().BeEquivalentTo(entitiesState.Select(e => e.Key));
            foreach (var entityEvent in events)
            {
                entityEvent.EventType.Should().Be(entitiesState[entityEvent.EntitySnapshot.Id]);
            }
        }

        [Test]
        public void TestLimit()
        {
            var startOffset = eventLogRepository.GetLastOffset();
            var expectedEntities = GenerateObjects(testObjectsCount).ToArray();
            sqlStorage.Create(expectedEntities);
            var notExpectedEntities = GenerateObjects(testObjectsCount).ToArray();
            sqlStorage.Create(notExpectedEntities);
            var events = eventLogRepository.GetEvents(startOffset, eventLogRepository.GetLastOffset(), expectedEntities.Length);
            events.Length.Should().Be(expectedEntities.Length);
            var actualSnapshots = events.Select(e => e.EntitySnapshot).ToArray();
            AssertUnorderedArraysEquality(actualSnapshots, expectedEntities);
        }

        [Test]
        public void TestOrdering()
        {
            var entity = GenerateObjects().First();
            var startOffset = eventLogRepository.GetLastOffset();
            sqlStorage.Create(entity);
            sqlStorage.Update(entity);
            sqlStorage.Delete(entity.Id);
            var events = eventLogRepository.GetEvents(startOffset, eventLogRepository.GetLastOffset(), 3);
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

        [Test, AndSqlStorageCleanUp(typeof(EventLogStorageElement))]
        public void TestGetMaxTimestampForOffset()
        {
            var maxForInitialOffset = eventLogRepository.GetMaxTimestampForOffset(initialOffset);
            sqlStorage.Create(GenerateObjects(testObjectsCount).ToArray());
            var newOffset = eventLogRepository.GetLastOffset();
            var maxForNewOffset = eventLogRepository.GetMaxTimestampForOffset(newOffset);

            var currentTime = sqlStorageTimeRepository.GetCurrentStorageTime();
            maxForInitialOffset.Should().BeBefore(maxForNewOffset);
            maxForNewOffset.Should().BeBefore(currentTime);
        }

        [Test, AndSqlStorageCleanUp(typeof(EventLogStorageElement))]
        public void TestGetLastOffsetForTimestamp()
        {
            sqlStorage.Create(GenerateObjects().First());
            var offset = eventLogRepository.GetLastOffset();
            var now = sqlStorageTimeRepository.GetCurrentStorageTime();
            sqlStorage.Create(GenerateObjects(testObjectsCount).ToArray());

            eventLogRepository.GetLastOffsetForTimestamp(now).Should().Be(offset);
        }

        private long initialOffset;

        private const int testObjectsCount = 100;

        [Injected]
        private readonly ISqlEventLogRepository<TEntity> eventLogRepository;

        [Injected]
        private readonly SqlStorageTimeRepository sqlStorageTimeRepository;
    }
}