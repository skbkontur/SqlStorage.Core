using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MoreLinq;

using NUnit.Framework;

using SKBKontur.Catalogue.EDI.SqlStorageCore.Storage;
using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests
{
    public class SearchTests : SqlStorageTestBase<TestValueTypedPropertiesStorageElement>
    {
        [Test]
        public void TestWriteSearchObjects()
        {
            var entities = GenerateObjects(testObjectsCount).ToArray();
            InternalTestWriteAndReadThroughMultipleThreads(entities, sqlStorage);
        }

        private static void InternalTestWriteAndReadThroughMultipleThreads(IReadOnlyCollection<TestValueTypedPropertiesStorageElement> objects, ISqlStorage<TestValueTypedPropertiesStorageElement> storage)
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
                                   var findResult = storage.Find(y => y.IntProperty == x.IntProperty);

                                   AssertUnorderedArraysEquality(findResult, objectsCaptured.Where(y => y.IntProperty == x.IntProperty));
                               });
                       });
        }

        private const int testObjectsCount = 100;
    }
}