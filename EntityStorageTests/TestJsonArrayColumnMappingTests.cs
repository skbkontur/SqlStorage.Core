using System;

using FluentAssertions;

using NUnit.Framework;

using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EntityStorageTests.TestEntities;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EntityStorageTests
{
    public class TestJsonArrayColumnMappingTests : EntityStorageTestBase<TestJsonArrayColumnElement>
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