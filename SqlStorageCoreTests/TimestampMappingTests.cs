using System;
using System.Threading.Tasks;

using FluentAssertions;

using NUnit.Framework;

using SkbKontur.Cassandra.TimeBasedUuid;
using SkbKontur.SqlStorageCore.Tests.TestEntities;
using SkbKontur.SqlStorageCore.Tests.TestWrappers;

namespace SkbKontur.SqlStorageCore.Tests
{
    [AndSqlStorageCleanUp(typeof(TestTimestampElement))]
    public class TimestampMappingTests : SqlStorageTestBase<TestTimestampElement, Guid>
    {
        [Test]
        public async Task TestReadWrite()
        {
            var entity = new TestTimestampElement {Id = Guid.NewGuid(), Timestamp = new Timestamp(new DateTime(2018, 07, 01, 0, 0, 0, DateTimeKind.Utc))};
            await sqlStorage.CreateOrUpdateAsync(entity);
            var actual = await sqlStorage.TryReadAsync(entity.Id);
            actual.Should().BeEquivalentTo(entity);
        }
    }
}