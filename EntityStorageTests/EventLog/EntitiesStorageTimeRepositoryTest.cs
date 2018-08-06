using System;
using System.Threading;

using FluentAssertions;

using NUnit.Framework;

using SKBKontur.Catalogue.EDI.SqlStorageCore.EventLog;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestWrappers;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EntityStorageTests.EventLog
{
    [EdiTestSuite, WithTestEntityStorage]
    public class EntitiesStorageTimeRepositoryTest
    {
        [Test]
        public void TestGetCurrentTime()
        {
            Action invocation = () => entitesStorageTimeRepository.GetCurrentStorageTime();
            invocation.Should().NotThrow();

            var currentTime = entitesStorageTimeRepository.GetCurrentStorageTime();
            currentTime.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Test]
        public void TestCurrentTimeIncreasing()
        {
            var start = entitesStorageTimeRepository.GetCurrentStorageTime();
            const int timeout = 500;
            Thread.Sleep(500);
            var end = entitesStorageTimeRepository.GetCurrentStorageTime();
            end.Subtract(start).Should().BeGreaterOrEqualTo(TimeSpan.FromMilliseconds(timeout));
        }
        [Injected]
        private readonly EntitesStorageTimeRepository entitesStorageTimeRepository;
    }
}