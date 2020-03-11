using System;
using System.Threading.Tasks;

using FluentAssertions;

using NUnit.Framework;

using SkbKontur.SqlStorageCore.Tests.TestEntities;

namespace SkbKontur.SqlStorageCore.Tests
{
    public class JsonColumnMappingTests : SqlStorageTestBase<TestJsonColumnElement, Guid>
    {
        [Test]
        public async Task TestWriteReadSimple()
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
            await sqlStorage.CreateOrUpdateAsync(entity);
            var actual = await sqlStorage.TryReadAsync(entity.Id);
            actual.Should().BeEquivalentTo(entity);
        }
    }
}