using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MoreLinq;

using NUnit.Framework;

using SkbKontur.SqlStorageCore.Tests.TestEntities;

namespace SkbKontur.SqlStorageCore.Tests
{
    public class SearchTests : SqlStorageTestBase<TestValueTypedPropertiesStorageElement, Guid>
    {
        [Test]
        public void TestWriteSearchObjects()
        {
            var entities = GenerateObjects(testObjectsCount).ToArray();
            InternalTestWriteAndReadThroughMultipleThreads(entities, sqlStorage);
        }

        private static void InternalTestWriteAndReadThroughMultipleThreads(IReadOnlyCollection<TestValueTypedPropertiesStorageElement> objects, IConcurrentSqlStorage<TestValueTypedPropertiesStorageElement, Guid> storage)
        {
            Parallel.ForEach(objects.Batch(objects.Count / 10), batch => batch.ForEach(e => storage.CreateOrUpdateAsync(e).GetAwaiter().GetResult()));

            var objectsCaptured = objects;
            objects.Batch(objects.Count / 10)
                   .AsParallel()
                   .ForAll(batch =>
                       {
                           var batchList = batch as IList<TestValueTypedPropertiesStorageElement> ?? batch.ToList();
                           batchList.ForEach(x =>
                               {
                                   var findResult = storage.FindAsync(y => y.IntProperty == x.IntProperty, int.MaxValue).GetAwaiter().GetResult();
                                   AssertUnorderedArraysEquality(findResult, objectsCaptured.Where(y => y.IntProperty == x.IntProperty));
                               });
                       });
        }

        private const int testObjectsCount = 100;
    }
}