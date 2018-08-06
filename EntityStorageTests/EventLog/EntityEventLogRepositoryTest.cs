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
using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EntityStorageTests.TestEntities;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EntityStorageTests.EventLog
{
    [TestFixture(typeof(TestValueTypedPropertiesStorageElement))]
    [TestFixture(typeof(TestJsonColumnElement))]
    [TestFixture(typeof(TestJsonArrayColumnElement))]
    public class EntityEventLogRepositoryTest<TEntity> : EntityStorageTestBase<TEntity>
        where TEntity : class, IIdentifiableEntity, new()
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
            entityStorage.Create(entity);
            var events = eventLogRepository.GetEvents(initialOffset, eventLogRepository.GetLastOffset(), 2);
            events.Length.Should().Be(1);
            var entityEvent = events.First();
            entityEvent.EntitySnapshot.Should().BeEquivalentTo(entity, equivalenceOptionsConfig);
            entityEvent.EventType.Should().Be(EntityEventType.Create);
        }

        [Test]
        public void TestCreateMultipleObjects()
        {
            var entities = GenerateObjects().Take(testObjectsCount).ToArray();
            entityStorage.Create(entities);
            var events = eventLogRepository.GetEvents(initialOffset, eventLogRepository.GetLastOffset(), entities.Length + 1);
            events.Length.Should().Be(entities.Length);
            events.All(e => e.EventType == EntityEventType.Create).Should().BeTrue();
            var actualEntities = events.Select(e => e.EntitySnapshot).ToArray();
            AssertUnorderedArraysEquality(actualEntities, entities);
        }

        [Test]
        public void TestUpdateSingleObject()
        {
            var entity = GenerateObjects().First();
            entityStorage.Create(entity);
            var offset = eventLogRepository.GetLastOffset();
            var updatedEntity = GenerateObjects().First();
            updatedEntity.Id = entity.Id;
            entityStorage.Update(updatedEntity);

            var events = eventLogRepository.GetEvents(offset, eventLogRepository.GetLastOffset(), 2);
            events.Length.Should().Be(1);
            var entityEvent = events.First();
            entityEvent.EntitySnapshot.Should().BeEquivalentTo(updatedEntity, equivalenceOptionsConfig);
            entityEvent.EventType.Should().Be(EntityEventType.Update);
        }

        [Test]
        public void TestUpdateMultipleObjects()
        {
            var entities = GenerateObjects().Take(testObjectsCount).ToArray();
            entityStorage.Create(entities);
            var offset = eventLogRepository.GetLastOffset();
            var updatedEntities = GenerateObjects().Take(testObjectsCount).Select((e, i) =>
                {
                    e.Id = entities[i].Id;
                    return e;
                }).ToArray();

            entityStorage.Update(updatedEntities);

            var events = eventLogRepository.GetEvents(offset, eventLogRepository.GetLastOffset(), entities.Length + 1);
            events.Length.Should().Be(entities.Length);
            events.All(e => e.EventType == EntityEventType.Update).Should().BeTrue();
            AssertUnorderedArraysEquality(events.Select(e => e.EntitySnapshot), updatedEntities);
        }

        [Test]
        public void TestDeleteSingleObject()
        {
            var entity = GenerateObjects().First();
            entityStorage.Create(entity);
            var offset = eventLogRepository.GetLastOffset();
            entityStorage.Delete(entity.Id);

            var events = eventLogRepository.GetEvents(offset, eventLogRepository.GetLastOffset(), 2);

            events.Length.Should().Be(1);
            var entityEvent = events.First();
            entityEvent.EntitySnapshot.Should().BeEquivalentTo(entity, equivalenceOptionsConfig);
            entityEvent.EventType.Should().Be(EntityEventType.Delete);
        }

        [Test]
        public void TestDeleteMultipleObjects()
        {
            var entities = GenerateObjects().Take(testObjectsCount).ToArray();
            entityStorage.Create(entities);
            var offset = eventLogRepository.GetLastOffset();
            entityStorage.Delete(entities.Select(e => e.Id).ToArray());

            var events = eventLogRepository.GetEvents(offset, eventLogRepository.GetLastOffset(), entities.Length + 1);
            events.Length.Should().Be(entities.Length);
            events.All(e => e.EventType == EntityEventType.Delete).Should().BeTrue();
            AssertUnorderedArraysEquality(events.Select(e => e.EntitySnapshot), entities);
        }

        [Test]
        public void TestCreateAndUpdateAndDeleteMultipleObjectsThroughMultipleThreads()
        {
            var random = new Random(DateTime.Now.Millisecond);
            var initialEntities = GenerateObjects().Take(testObjectsCount).ToArray();
            entityStorage.Create(initialEntities);
            var entitiesState = new ConcurrentDictionary<Guid, EntityEventType>();
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
                            entityStorage.Update(updated);
                            entitiesState.TryAdd(entity.Id, EntityEventType.Update);
                            continue;
                        case 1:
                            entityStorage.Delete(entity.Id);
                            entitiesState.TryAdd(entity.Id, EntityEventType.Delete);
                            continue;
                        case 2:
                            var newEntity = GenerateObjects().First();
                            entityStorage.Create(newEntity);
                            entitiesState.TryAdd(newEntity.Id, EntityEventType.Create);
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

        [Test, AndEntityStorageCleanUp(typeof(EventLogEntity))]
        public void TestGetMaxTimestampForOffset()
        {
            var maxForInitialOffset = eventLogRepository.GetMaxTimestampForOffset(initialOffset);
            entityStorage.Create(GenerateObjects().Take(testObjectsCount).ToArray());
            var newOffset = eventLogRepository.GetLastOffset();
            var maxForNewOffset = eventLogRepository.GetMaxTimestampForOffset(newOffset);

            var currentTime = entitesStorageTimeRepository.GetCurrentStorageTime();
            maxForInitialOffset.Should().BeBefore(maxForNewOffset);
            maxForNewOffset.Should().BeBefore(currentTime);
        }

        [Test, AndEntityStorageCleanUp(typeof(EventLogEntity))]
        public void TestGetLastOffsetForTimestamp()
        {
            entityStorage.Create(GenerateObjects().First());
            var offset = eventLogRepository.GetLastOffset();
            var now = entitesStorageTimeRepository.GetCurrentStorageTime();
            entityStorage.Create(GenerateObjects().Take(testObjectsCount).ToArray());

            eventLogRepository.GetLastOffsetForTimestamp(now).Should().Be(offset);
        }

        private long initialOffset;

        private const int testObjectsCount = 100;

        [Injected]
        private readonly IEntitiesEventLogRepository<TEntity> eventLogRepository;

        [Injected]
        private readonly EntitesStorageTimeRepository entitesStorageTimeRepository;
    }
}