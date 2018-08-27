using System;

using FluentAssertions;

using NUnit.Framework;

using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests
{
    public class JsonColumnMappingTests : SqlStorageTestBase<TestJsonColumnElement>
    {
        [Test]
        public void TestWriteReadSimple()
        {
            var entity = new TestJsonColumnElement
                {
                    Id = Guid.NewGuid(),
                    ComplexColumn = new TestComplexColumnElement
                        {
                            IntProperty = 1289,
                            StringProperty = "testComplexType",
                        }
                };
            sqlStorage.CreateOrUpdate(entity);
            var actual = sqlStorage.TryRead(entity.Id);
            actual.Should().BeEquivalentTo(entity);
        }
    }
}