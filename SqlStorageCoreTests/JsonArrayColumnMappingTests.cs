using System;
using System.Threading.Tasks;

using FluentAssertions;

using NUnit.Framework;

using SkbKontur.SqlStorageCore.Tests.TestEntities;

namespace SkbKontur.SqlStorageCore.Tests
{
    public class JsonArrayColumnMappingTests : SqlStorageTestBase<TestJsonArrayColumnElement, Guid>
    {
        [Test]
        public async Task TestWriteReadSimple()
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
            await sqlStorage.CreateOrUpdateAsync(entity);
            var actual = await sqlStorage.TryReadAsync(entity.Id);
            actual.Should().BeEquivalentTo(entity);
        }
    }
}