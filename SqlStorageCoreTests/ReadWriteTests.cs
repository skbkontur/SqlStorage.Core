using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MoreLinq;

using NUnit.Framework;

using SkbKontur.SqlStorageCore.Tests.TestEntities;
using SkbKontur.SqlStorageCore.Tests.TestWrappers;

namespace SkbKontur.SqlStorageCore.Tests
{
    [AndSqlStorageCleanUp(typeof(TestValueTypedPropertiesStorageElement))]
    public class ReadWriteTests : SqlStorageTestBase<TestValueTypedPropertiesStorageElement, Guid>
    {
        [Test]
        public async Task TestReadWriteSingleObject()
        {
            var entity = GenerateObjects().First();
            await sqlStorage.CreateOrUpdateAsync(entity);
            var actualObject = await sqlStorage.TryReadAsync(entity.Id);

            actualObject.Should().BeEquivalentTo(entity, equivalenceOptionsConfig);
        }

        [Test]
        public async Task TestRewriteSingleObject()
        {
            var entity = GenerateObjects().First();
            await sqlStorage.CreateOrUpdateAsync(entity);
            Func<Task> rewrite = async () => await sqlStorage.CreateOrUpdateAsync(entity);
            await rewrite.Should().NotThrowAsync();
            entity.IntProperty++;
            await sqlStorage.CreateOrUpdateAsync(entity);
            var actualObject = await sqlStorage.TryReadAsync(entity.Id);
            actualObject.Should().BeEquivalentTo(entity, equivalenceOptionsConfig);
        }

        [Test]
        public async Task TestRewriteMultipleObjects()
        {
            var entities = GenerateObjects(testObjectsCount).ToArray();
            await sqlStorage.CreateOrUpdateAsync(entities);
            var moreEntities = GenerateObjects(testObjectsCount).ToArray();
            await sqlStorage.CreateOrUpdateAsync(entities.Concat(moreEntities).ToArray());
            (await sqlStorage.ReadAllAsync()).Length.Should().Be(testObjectsCount * 2);
        }

        [Test]
        public async Task TestReadNotExistingObject()
        {
            var entity = GenerateObjects().First();
            var actualObject = await sqlStorage.TryReadAsync(entity.Id);
            actualObject.Should().BeNull();
        }

        [Test]
        public async Task TestDeleteSingleObject()
        {
            var entity = GenerateObjects().First();
            await sqlStorage.CreateOrUpdateAsync(entity);
            await sqlStorage.DeleteAsync(entity.Id);
            var actualObject = await sqlStorage.TryReadAsync(entity.Id);
            actualObject.Should().BeNull();
        }

        [Test]
        public async Task TestDeleteNonExistingObject()
        {
            Func<Task> deletion = async () => await sqlStorage.DeleteAsync(Guid.NewGuid());
            await deletion.Should().NotThrowAsync();
        }

        [Test]
        public async Task TestDeleteMultipleObjects()
        {
            var entities = GenerateObjects(testObjectsCount).ToArray();
            await sqlStorage.CreateOrUpdateAsync(entities);
            var ids = entities.Select(e => e.Id).Concat(new[] {Guid.NewGuid()}).ToArray();
            Func<Task> deletion = async () => await sqlStorage.DeleteAsync(ids);
            await deletion.Should().NotThrowAsync();
            (await sqlStorage.TryReadAsync(ids)).Length.Should().Be(0);
        }

        [Test]
        public async Task TestCreateSingleObject()
        {
            var entity = GenerateObjects().First();
            await sqlStorage.CreateOrUpdateAsync(entity);
            (await sqlStorage.TryReadAsync(entity.Id))
                .Should()
                .BeEquivalentTo(entity, equivalenceOptionsConfig);
        }

        [Test]
        public async Task TestCreateMultipleObjects()
        {
            var entities = GenerateObjects(testObjectsCount).ToArray();
            await sqlStorage.CreateOrUpdateAsync(entities);
            var actualEntities = await sqlStorage.TryReadAsync(entities.Select(e => e.Id).ToArray());
            AssertUnorderedArraysEquality(actualEntities, entities);
        }

        [Test]
        public async Task TestRecreateSingleObject()
        {
            var entity = GenerateObjects().First();
            await sqlStorage.CreateOrUpdateAsync(entity);
            Func<Task> repeatCreation = async () => await sqlStorage.CreateOrUpdateAsync(entity);
            await repeatCreation.Should().NotThrowAsync();
            (await sqlStorage.ReadAllAsync()).Length.Should().Be(1);
        }

        [Test]
        public async Task TestRecreateMultipleObjects()
        {
            var entities = GenerateObjects(testObjectsCount).ToArray();
            await sqlStorage.CreateOrUpdateAsync(entities);
            Func<Task> repeatCreation = async () => await sqlStorage.CreateOrUpdateAsync(entities);
            await repeatCreation.Should().NotThrowAsync();
            (await sqlStorage.ReadAllAsync()).Length.Should().Be(entities.Length);
        }

        [Test]
        public async Task TestUpdateSingleObject()
        {
            var entity = GenerateObjects().First();
            await sqlStorage.CreateOrUpdateAsync(entity);
            entity.IntProperty++;
            await sqlStorage.CreateOrUpdateAsync(entity);
            (await sqlStorage.TryReadAsync(entity.Id))
                .Should()
                .BeEquivalentTo(entity, equivalenceOptionsConfig);
        }

        [Test]
        public async Task TestUpdateMultipleObjects()
        {
            var entities = GenerateObjects(testObjectsCount).ToArray();
            await sqlStorage.CreateOrUpdateAsync(entities);
            foreach (var entity in entities)
            {
                entity.IntProperty++;
            }
            await sqlStorage.CreateOrUpdateAsync(entities);
            var actualEntities = await sqlStorage.TryReadAsync(entities.Select(e => e.Id).ToArray());
            AssertUnorderedArraysEquality(actualEntities, entities);
        }

        [Test]
        public async Task TestReadWriteMultipleObjects()
        {
            var objects = GenerateObjects(testObjectsCount).ToArray();
            await sqlStorage.CreateOrUpdateAsync(objects);
            var actualObjects = await sqlStorage.TryReadAsync(objects.Select(x => x.Id).ToArray());
            AssertUnorderedArraysEquality(actualObjects, objects);
        }

        [Test]
        public async Task TestReadWriteMultipleObjectsThroughSingleWrites()
        {
            var objects = GenerateObjects(testObjectsCount).ToArray();
            foreach (var obj in objects)
                await sqlStorage.CreateOrUpdateAsync(obj);
            var actualObjects = await sqlStorage.TryReadAsync(objects.Select(x => x.Id).ToArray());
            AssertUnorderedArraysEquality(actualObjects, objects);
        }

        [Test]
        public async Task TestReadThroughSingleReadAndMultipleWrite()
        {
            var objects = GenerateObjects(testObjectsCount).ToArray();
            await sqlStorage.CreateOrUpdateAsync(objects);

            var actualObjects = new List<TestValueTypedPropertiesStorageElement>();
            foreach (var id in objects.Select(x => x.Id))
            {
                actualObjects.Add(await sqlStorage.TryReadAsync(id) ?? throw new AssertionException("Unexpected null"));
            }
            AssertUnorderedArraysEquality(actualObjects.ToArray(), objects);
        }

        [Test]
        public async Task TestWriteThroughMultipleThreadsAndCheckResultThroughSingleReads()
        {
            var objects = GenerateObjects(testObjectsCount * 10).ToArray();
            Parallel.ForEach(
                objects.Batch(testObjectsCount),
                batch => batch.ForEach(x => sqlStorage.CreateOrUpdateAsync(x).GetAwaiter().GetResult()));
            var actualObjects = await sqlStorage.TryReadAsync(objects.Select(x => x.Id).ToArray());
            AssertUnorderedArraysEquality(actualObjects, objects);
        }

        [Test]
        public void TestWriteAndReadThroughMultipleThreads()
        {
            var objects = GenerateObjects(testObjectsCount * 10).ToArray();
            Parallel.ForEach(objects.Batch(objects.Length / 10), batch => batch.ForEach(e => sqlStorage.CreateOrUpdateAsync(e).GetAwaiter().GetResult()));

            var actualObjects = objects.Batch(objects.Length / 10)
                                       .AsParallel()
                                       .Select(batch => batch.Select(x => sqlStorage.TryReadAsync(x.Id).GetAwaiter().GetResult()))
                                       .SelectMany(x => x);

            AssertUnorderedArraysEquality(actualObjects, objects);
        }

        [Test]
        public async Task TestWriteAndDeleteAndReadThroughMultipleThreads()
        {
            var objects = GenerateObjects(testObjectsCount).ToArray();
            var rnd = new Random();
            var objectToDelete = objects.OrderBy(x => rnd.Next()).Take(testObjectsCount / 4).ToArray();
            var creationTasks = objects.Batch(objects.Length / 10)
                                       .Select(batch => sqlStorage.CreateOrUpdateAsync(batch.ToArray()))
                                       .ToArray();
            await Task.WhenAll(creationTasks).ConfigureAwait(false);

            var deletionTasks = objectToDelete.Batch(objectToDelete.Length / 5)
                                              .Select(batch => sqlStorage.DeleteAsync(batch.Select(e => e.Id).ToArray()))
                                              .ToArray();
            await Task.WhenAll(deletionTasks).ConfigureAwait(false);

            var actualObjects = objects.ToArray().Batch(objects.ToArray().Length / 10)
                                       .AsParallel()
                                       .Select(batch => batch.Select(x => sqlStorage.TryReadAsync(x.Id).GetAwaiter().GetResult()))
                                       .SelectMany(x => x)
                                       .Where(x => x != null);

            AssertUnorderedArraysEquality(actualObjects, objects.Except(objectToDelete));
        }

        [Test]
        public async Task TestWriteAndDeleteAndReadAll()
        {
            var objects = GenerateObjects(testObjectsCount).ToArray();
            var rnd = new Random();
            var objectsToDelete = objects.OrderBy(x => rnd.Next()).Take(testObjectsCount / 3).ToArray();
            await sqlStorage.CreateOrUpdateAsync(objects);
            await sqlStorage.DeleteAsync(objectsToDelete.Select(o => o.Id).ToArray());
            var allActualObjects = await sqlStorage.ReadAllAsync();
            allActualObjects.Length.Should().Be(objects.Length - objectsToDelete.Length);
            allActualObjects.Should().NotContain(objectsToDelete);
        }

        private const int testObjectsCount = 100;
    }
}