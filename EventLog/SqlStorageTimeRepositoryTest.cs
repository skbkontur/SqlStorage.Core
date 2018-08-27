using System;
using System.Threading;

using FluentAssertions;

using NUnit.Framework;

using SKBKontur.Catalogue.EDI.SqlStorageCore.EventLog;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestWrappers;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EventLog
{
    [EdiTestSuite, WithTestSqlStorage]
    public class SqlStorageTimeRepositoryTest
    {
        [Test]
        public void TestGetCurrentTime()
        {
            Action invocation = () => sqlStorageTimeRepository.GetCurrentStorageTime();
            invocation.Should().NotThrow();

            var currentTime = sqlStorageTimeRepository.GetCurrentStorageTime();
            currentTime.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Test]
        public void TestCurrentTimeIncreasing()
        {
            var start = sqlStorageTimeRepository.GetCurrentStorageTime();
            const int timeout = 500;
            Thread.Sleep(500);
            var end = sqlStorageTimeRepository.GetCurrentStorageTime();
            end.Subtract(start).Should().BeGreaterOrEqualTo(TimeSpan.FromMilliseconds(timeout));
        }
        [Injected]
        private readonly SqlStorageTimeRepository sqlStorageTimeRepository;
    }
}