using System;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;

using SKBKontur.Catalogue.EDI.SqlStorageCore.EventLog;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.Objects;
using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EventLog
{
    public class TimestampPropertyTest : SqlStorageTestBase<TestTimestampElement>
    {
        [Test]
        public void TestReadEvent()
        {
            var entity = new TestTimestampElement {Id = Guid.NewGuid(), Timestamp = new Timestamp(new DateTime(2018, 08, 06, 12, 7, 5, DateTimeKind.Utc))};
            var offset = eventLogRepository.GetLastOffset();
            sqlStorage.Create(entity);
            var events = eventLogRepository.GetEvents(offset, eventLogRepository.GetLastOffset(), 2);
            events.Length.Should().Be(1);
            events.First().EntitySnapshot.Should().BeEquivalentTo(entity);
        }

        [Injected]
        private readonly ISqlEventLogRepository<TestTimestampElement> eventLogRepository;
    }
}