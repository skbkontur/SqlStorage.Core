using System;

using FluentAssertions;

using NUnit.Framework;

using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests
{
    public class JsonArrayColumnMappingTests : SqlStorageTestBase<TestJsonArrayColumnElement>
    {
        [Test]
        public void TestWriteReadSimple()
        {
            var entity = new TestJsonArrayColumnElement
                {
                    Id = Guid.NewGuid(),
                    ComplexArrayColumn = new[]
                        {
                            new TestComplexColumnElement
                                {
                                    IntProperty = 1289,
                                    StringProperty = "testComplexType",
                                },
                            new TestComplexColumnElement
                                {
                                    IntProperty = 32323,
                                    StringProperty = "testComplexType2",
                                }
                        }
                };
            sqlStorage.CreateOrUpdate(entity);
            var actual = sqlStorage.TryRead(entity.Id);
            actual.Should().BeEquivalentTo(entity);
        }
    }
}