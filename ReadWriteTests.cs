using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using MoreLinq;

using NUnit.Framework;

using SKBKontur.Catalogue.EDI.SqlStorageCore;
using SKBKontur.Catalogue.EDIFunctionalTests.Commons.TestWrappers;
using SKBKontur.Catalogue.Linq;
using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests
{
    [AndSqlStorageCleanUp(typeof(TestValueTypedPropertiesStorageElement))]
    public class ReadWriteTests : SqlStorageTestBase<TestValueTypedPropertiesStorageElement, Guid>
    {
        [Test]
        public void TestReadWriteSingleObject()
        {
            var entity = GenerateObjects().First();
            sqlStorage.CreateOrUpdate(entity);
            var actualObject = sqlStorage.TryRead(entity.Id);

            actualObject.Should().BeEquivalentTo(entity, equivalenceOptionsConfig);
        }

        [Test]
        public void TestRewriteSingleObject()
        {
            var entity = GenerateObjects().First();
            sqlStorage.CreateOrUpdate(entity);
            Action rewrite = () => sqlStorage.CreateOrUpdate(entity);
            rewrite.Should().NotThrow();
            entity.IntProperty++;
            sqlStorage.CreateOrUpdate(entity);
            var actualObject = sqlStorage.TryRead(entity.Id);
            actualObject.Should().BeEquivalentTo(entity, equivalenceOptionsConfig);
        }

        [Test]
        public void TestRewriteMultipleObjects()
        {
            var entities = GenerateObjects(testObjectsCount).ToArray();
            sqlStorage.CreateOrUpdate(entities);
            var moreEntities = GenerateObjects(testObjectsCount).ToArray();
            sqlStorage.CreateOrUpdate(entities.Concat(moreEntities).ToArray());
            sqlStorage.ReadAll().Length.Should().Be(testObjectsCount * 2);
        }

        [Test]
        public void TestReadNotExistingObject()
        {
            var entity = GenerateObjects().First();
            var actualObject = sqlStorage.TryRead(entity.Id);
            actualObject.Should().BeNull();
        }

        [Test]
        public void TestDeleteSingleObject()
        {
            var entity = GenerateObjects().First();
            sqlStorage.CreateOrUpdate(entity);
            sqlStorage.Delete(entity.Id);
            var actualObject = sqlStorage.TryRead(entity.Id);
            actualObject.Should().BeNull();
        }

        [Test]
        public void TestDeleteNonExistingObject()
        {
            Action deletion = () => sqlStorage.Delete(Guid.NewGuid());
            deletion.Should().NotThrow();
        }

        [Test]
        public void TestDeleteMultipleObjects()
        {
            var entities = GenerateObjects(testObjectsCount).ToArray();
            sqlStorage.CreateOrUpdate(entities);
            var ids = entities.Select(e => e.Id).Concat(new[] {Guid.NewGuid()}).ToArray();
            Action deletion = () => sqlStorage.Delete(ids);
            deletion.Should().NotThrow();
            sqlStorage.TryRead(ids).Length.Should().Be(0);
        }

        [Test]
        public void TestCreateSingleObject()
        {
            var entity = GenerateObjects().First();
            sqlStorage.CreateOrUpdate(entity);
            sqlStorage.TryRead(entity.Id)
                      .Should()
                      .BeEquivalentTo(entity, equivalenceOptionsConfig);
        }

        [Test]
        public void TestCreateMultipleObjects()
        {
            var entities = GenerateObjects(testObjectsCount).ToArray();
            sqlStorage.CreateOrUpdate(entities);
            var actualEntities = sqlStorage.TryRead(entities.Select(e => e.Id).ToArray());
            AssertUnorderedArraysEquality(actualEntities, entities);
        }

        [Test]
        public void TestRecreateSingleObject()
        {
            var entity = GenerateObjects().First();
            sqlStorage.CreateOrUpdate(entity);
            Action repeatCreation = () => sqlStorage.CreateOrUpdate(entity);
            repeatCreation.Should().NotThrow();
            sqlStorage.ReadAll().Length.Should().Be(1);
        }

        [Test]
        public void TestRecreateMultipleObjects()
        {
            var entities = GenerateObjects(testObjectsCount).ToArray();
            sqlStorage.CreateOrUpdate(entities);
            Action repeatCreation = () => sqlStorage.CreateOrUpdate(entities);
            repeatCreation.Should().NotThrow();
            sqlStorage.ReadAll().Length.Should().Be(entities.Length);
        }

        [Test]
        public void TestUpdateSingleObject()
        {
            var entity = GenerateObjects().First();
            sqlStorage.CreateOrUpdate(entity);
            entity.IntProperty++;
            sqlStorage.CreateOrUpdate(entity);
            sqlStorage.TryRead(entity.Id)
                      .Should()
                      .BeEquivalentTo(entity, equivalenceOptionsConfig);
        }

        [Test]
        public void TestUpdateMultipleObjects()
        {
            var entities = GenerateObjects(testObjectsCount).ToArray();
            sqlStorage.CreateOrUpdate(entities);
            foreach (var entity in entities)
            {
                entity.IntProperty++;
            }
            sqlStorage.CreateOrUpdate(entities);
            var actualEntities = sqlStorage.TryRead(entities.Select(e => e.Id).ToArray());
            AssertUnorderedArraysEquality(actualEntities, entities);
        }

        [Test]
        public void TestReadWriteMultipleObjects()
        {
            var objects = GenerateObjects(testObjectsCount).ToArray();
            sqlStorage.CreateOrUpdate(objects);
            var actualObjects = sqlStorage.TryRead(objects.Select(x => x.Id).ToArray());
            AssertUnorderedArraysEquality(actualObjects, objects);
        }

        [Test]
        public void TestReadWriteMultipleObjectsThroughSingleWrites()
        {
            var objects = GenerateObjects(testObjectsCount).ToArray();
            objects.ForEach(x => sqlStorage.CreateOrUpdate(x));
            var actualObjects = sqlStorage.TryRead(objects.Select(x => x.Id).ToArray());
            AssertUnorderedArraysEquality(actualObjects, objects);
        }

        [Test]
        public void TestReadThroughSingleReadAndMultipleWrite()
        {
            var objects = GenerateObjects(testObjectsCount).ToArray();
            sqlStorage.CreateOrUpdate(objects);
            var actualObjects = objects.Select(x => x.Id).Select(x => sqlStorage.TryRead(x)).ToArray();
            AssertUnorderedArraysEquality(actualObjects, objects);
        }

        [Test]
        public void TestWriteThroughMultipleThreadsAndCheckResultThroughSingleReads()
        {
            var objects = GenerateObjects(testObjectsCount * 10).ToArray();
            Parallel.ForEach(objects.Batch(testObjectsCount), batch => batch.ForEach(x => sqlStorage.CreateOrUpdate(x)));
            var actualObjects = sqlStorage.TryRead(objects.Select(x => x.Id).ToArray());
            AssertUnorderedArraysEquality(actualObjects, objects);
        }

        [Test]
        public void TestWriteAndReadThroughMultipleThreads()
        {
            InternalTestWriteAndReadThroughMultipleThreads(GenerateObjects(testObjectsCount * 10).ToArray(), sqlStorage);
        }

        [Test]
        public void TestWriteAndDeleteAndReadThroughMultipleThreads()
        {
            var objects = GenerateObjects(testObjectsCount).ToArray();
            var objectToDelete = objects.RandomElements(new Random(), testObjectsCount / 3).ToArray();
            InternalTestWriteAndDeleteAndReadThroughMultipleThreads(objects.ToArray(), objectToDelete.ToArray(), sqlStorage);
        }

        [Test]
        public void TestWriteAndDeleteAndReadAll()
        {
            var objects = GenerateObjects(testObjectsCount).ToArray();
            var objectsToDelete = objects.RandomElements(new Random(), testObjectsCount / 3).ToArray();
            sqlStorage.CreateOrUpdate(objects);
            sqlStorage.Delete(objectsToDelete.Select(o => o.Id).ToArray());
            var allActualObjects = sqlStorage.ReadAll();
            allActualObjects.Length.Should().Be(objects.Length - objectsToDelete.Length);
            allActualObjects.Should().NotContain(objectsToDelete);
        }

        private static void InternalTestWriteAndDeleteAndReadThroughMultipleThreads(TestValueTypedPropertiesStorageElement[] objects, TestValueTypedPropertiesStorageElement[] objectsToDelete, ISqlStorage<TestValueTypedPropertiesStorageElement, Guid> storage)
        {
            Parallel.ForEach(objects.Batch(objects.Length / 10), batch => batch.ForEach(e => storage.CreateOrUpdate(e)));
            Parallel.ForEach(objectsToDelete.Batch(objectsToDelete.Length / 10), batch => batch.ForEach(x => storage.Delete(new[] {x.Id})));

            var actualObjects = objects.Batch(objects.Length / 10)
                                       .AsParallel()
                                       .Select(batch => batch.Select(x => storage.TryRead(x.Id)))
                                       .SelectMany(x => x)
                                       .Where(x => x != null);

            AssertUnorderedArraysEquality(actualObjects, objects.Except(objectsToDelete).ToArray());
        }

        private static void InternalTestWriteAndReadThroughMultipleThreads(TestValueTypedPropertiesStorageElement[] objects, ISqlStorage<TestValueTypedPropertiesStorageElement, Guid> storage)
        {
            Parallel.ForEach(objects.Batch(objects.Length / 10), batch => batch.ForEach(e => storage.CreateOrUpdate(e)));

            var actualObjects = objects.Batch(objects.Length / 10)
                                       .AsParallel()
                                       .Select(batch => batch.Select(x => storage.TryRead(x.Id)))
                                       .SelectMany(x => x);

            AssertUnorderedArraysEquality(actualObjects, objects);
        }

        private const int testObjectsCount = 100;
    }
}