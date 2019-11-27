using System;
using System.Linq;

using FluentAssertions;

using GroboContainer.NUnitExtensions;

using NUnit.Framework;

using SkbKontur.Cassandra.TimeBasedUuid;
using SkbKontur.SqlStorageCore.EventLog;
using SkbKontur.SqlStorageCore.Tests.TestEntities;
using SkbKontur.SqlStorageCore.Tests.TestWrappers;

namespace SkbKontur.SqlStorageCore.Tests.EventLog
{
    [AndSqlStorageCleanUp(typeof(SqlEventLogEntry))]
    public class TimestampPropertyTest : SqlStorageTestBase<TestTimestampElement, Guid>
    {
        [Test]
        public void TestReadEvent()
        {
            var entity = new TestTimestampElement {Id = Guid.NewGuid(), Timestamp = new Timestamp(new DateTime(2018, 08, 06, 12, 7, 5, DateTimeKind.Utc))};
            sqlStorage.CreateOrUpdate(entity);
            var events = eventLogRepository.GetEvents(null, 2);
            events.Length.Should().Be(1);
            events.First().EntitySnapshot.Should().BeEquivalentTo(entity);
        }

        [Injected]
        private readonly ISqlEventLogRepository<TestTimestampElement, Guid> eventLogRepository;
    }
}