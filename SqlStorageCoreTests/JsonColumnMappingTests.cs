using System;

using FluentAssertions;

using NUnit.Framework;

using SkbKontur.SqlStorageCore.Tests.TestEntities;

namespace SkbKontur.SqlStorageCore.Tests
{
    public class JsonColumnMappingTests : SqlStorageTestBase<TestJsonColumnElement, Guid>
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