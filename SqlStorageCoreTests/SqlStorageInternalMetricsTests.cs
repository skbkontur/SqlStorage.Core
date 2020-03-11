using System;
using System.Threading.Tasks;

using FluentAssertions;

using GroboContainer.NUnitExtensions;

using NUnit.Framework;

using SkbKontur.SqlStorageCore.Tests.TestEntities;
using SkbKontur.SqlStorageCore.Tests.TestWrappers;

using Vostok.Metrics;
using Vostok.Metrics.Models;
using Vostok.Metrics.Senders;

namespace SkbKontur.SqlStorageCore.Tests
{
    [GroboTestSuite, WithTestSqlStorage]
    public class SqlStorageInternalMetricsTests
    {
        [Test]
        public async Task TestReadTimer()
        {
            MetricEvent? metricEvent = null;
            var metricContext = new MetricContext(new MetricContextConfig(new AdHocMetricEventSender(e => metricEvent = e)));
            var storage = new ConcurrentSqlStorage<TestValueTypedPropertiesStorageElement, Guid>(createDbContext, metricContext);
            var entity = new TestValueTypedPropertiesStorageElement {StringProperty = "d65dfy556"};
            storage.CreateOrUpdate(entity);
            storage.TryRead(entity.Id);

            metricEvent.Should().NotBeNull();
            metricEvent!.Unit.Should().Be("seconds");
            metricEvent!.Value.Should().BeLessThan(1);
        }



        [Injected]
        private readonly Func<SqlDbContext> createDbContext;
    }
}