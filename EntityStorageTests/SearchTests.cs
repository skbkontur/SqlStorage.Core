using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MoreLinq;

using NUnit.Framework;

using SKBKontur.Catalogue.EDI.SqlStorageCore.Storage;
using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EntityStorageTests.TestEntities;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EntityStorageTests
{
    public class SearchTests : EntityStorageTestBase<TestValueTypedPropertiesStorageElement>
    {
        [Test]
        public void TestWriteSearchObjects()
        {
            var entities = GenerateObjects().Take(testObjectsCount).ToArray();
            InternalTestWriteAndReadThroughMultipleThreads(entities, entityStorage);
        }

        private static void InternalTestWriteAndReadThroughMultipleThreads(IReadOnlyCollection<TestValueTypedPropertiesStorageElement> objects, IEntityStorage<TestValueTypedPropertiesStorageElement> storage)
        {
            Parallel.ForEach(objects.Batch(objects.Count / 10), batch => batch.ForEach(storage.CreateOrUpdate));

            var objectsCaptured = objects;
            objects.Batch(objects.Count / 10)
                   .AsParallel()
                   .ForAll(batch =>
                       {
                           var batchList = batch as IList<TestValueTypedPropertiesStorageElement> ?? batch.ToList();
                           batchList.ForEach(x =>
                               {
                                   var findResult = storage.Find(y => y.IntProperty == x.IntProperty, 0, 1000);

                                   AssertUnorderedArraysEquality(findResult, objectsCaptured.Where(y => y.IntProperty == x.IntProperty));
                               });
                       });
        }

        private const int testObjectsCount = 100;
    }
}