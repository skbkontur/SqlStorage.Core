using System;

using FluentAssertions;

using NUnit.Framework;

using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EntityStorageTests.TestEntities;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EntityStorageTests
{
    public class TestJsonColumnMappingTests : EntityStorageTestBase<TestJsonColumnElement>
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
            entityStorage.CreateOrUpdate(entity);
            var actual = entityStorage.TryRead(entity.Id);
            actual.Should().BeEquivalentTo(entity);
        }
    }
}