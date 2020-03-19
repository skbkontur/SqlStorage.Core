using System;
using System.Linq;
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
        public async Task TestTimerRead()
        {
            MetricEvent? metricEvent = null;
            var metricContext = new MetricContext(new MetricContextConfig(new AdHocMetricEventSender(e => metricEvent = e)));
            var storage = GetStorage(metricContext);
            var entity = GetEntity();
            storage.CreateOrUpdate(entity);
            storage.TryRead(entity.Id);

            metricEvent.Should().NotBeNull();
            metricEvent!.Unit.Should().Be("seconds");
            metricEvent!.Value.Should().BeLessThan(1);
        }
        [Test]
        public async Task TestTimerReadMany()
        {
            MetricEvent? metricEvent = null;
            var metricContext = new MetricContext(new MetricContextConfig(new AdHocMetricEventSender(e => metricEvent = e)));
            var storage = GetStorage(metricContext);

            var entities = GetEntities();
            storage.CreateOrUpdate(entities);
            storage.TryRead(entities.Select(x => x.Id).ToArray());

            metricEvent.Should().NotBeNull();
            metricEvent!.Unit.Should().Be("seconds");
            metricEvent!.Value.Should().BeLessThan(1);
        }

        [Test]
        public async Task TestTimerReadAll()
        {
            MetricEvent? metricEvent = null;
            var metricContext = new MetricContext(new MetricContextConfig(new AdHocMetricEventSender(e => metricEvent = e)));
            var storage = GetStorage(metricContext);
            var entities = GetEntities();
            storage.CreateOrUpdate(entities);
            storage.ReadAll();

            metricEvent.Should().NotBeNull();
            metricEvent!.Unit.Should().Be("seconds");
            metricEvent!.Value.Should().BeLessThan(1);
        }

        [Test]
        public async Task TestTimerDelete()
        {
            MetricEvent? metricEvent = null;
            var metricContext = new MetricContext(new MetricContextConfig(new AdHocMetricEventSender(e => metricEvent = e)));
            var storage = GetStorage(metricContext);
            var entity = GetEntity();
            storage.CreateOrUpdate(entity);
            storage.Delete(entity.Id);

            metricEvent.Should().NotBeNull();
            metricEvent!.Unit.Should().Be("seconds");
            metricEvent!.Value.Should().BeLessThan(1);
        }

        [Test]
        public async Task TestTimerReadWithoutCreation()
        {
            MetricEvent? metricEvent = null;
            var metricContext = new MetricContext(new MetricContextConfig(new AdHocMetricEventSender(e => metricEvent = e)));
            var storage = GetStorage(metricContext);
            var entity = GetEntity();
            storage.TryRead(entity.Id);

            metricEvent.Should().NotBeNull();
            metricEvent!.Unit.Should().Be("seconds");
            metricEvent!.Value.Should().BeLessThan(1);
        }

        [Test]
        public async Task TestTimerDeleteWithoutCreation()
        {
            MetricEvent? metricEvent = null;
            var metricContext = new MetricContext(new MetricContextConfig(new AdHocMetricEventSender(e => metricEvent = e)));
            var storage = GetStorage(metricContext);
            var entity = GetEntity();
            storage.Delete(entity.Id);

            metricEvent.Should().NotBeNull();
            metricEvent!.Unit.Should().Be("seconds");
            metricEvent!.Value.Should().BeLessThan(1);
        }

        [Test]
        public async Task TestTimerCreateMany()
        {
            MetricEvent? metricEvent = null;
            var metricContext = new MetricContext(new MetricContextConfig(new AdHocMetricEventSender(e => metricEvent = e)));
            var storage = GetStorage(metricContext);
            var entities = GetEntities();
            storage.CreateOrUpdate(entities);

            metricEvent.Should().NotBeNull();
            metricEvent!.Unit.Should().Be("seconds");
            metricEvent!.Value.Should().BeLessThan(1);
        }

        [Test]
        public async Task TestTimerFind()
        {
            MetricEvent? metricEvent = null;
            var metricContext = new MetricContext(new MetricContextConfig(new AdHocMetricEventSender(e => metricEvent = e)));
            var storage = GetStorage(metricContext);

            var entities = GetEntities();
            storage.CreateOrUpdate(entities);
            storage.Find(e => e.StringProperty == stringProperty, entitiesCount);

            metricEvent.Should().NotBeNull();
            metricEvent!.Unit.Should().Be("seconds");
            metricEvent!.Value.Should().BeLessThan(1);
        }


        private const string stringProperty = "d65dfy556";
        private const int entitiesCount = 1000;

        private TestValueTypedPropertiesStorageElement[] GetEntities() =>
             Enumerable.Range(0, entitiesCount).Select(x => GetEntity()).ToArray();

        private TestValueTypedPropertiesStorageElement GetEntity() =>
            new TestValueTypedPropertiesStorageElement { Id = Guid.NewGuid(), StringProperty = stringProperty };

        private ConcurrentSqlStorage<TestValueTypedPropertiesStorageElement, Guid> GetStorage(MetricContext metricContext) =>
            new ConcurrentSqlStorage<TestValueTypedPropertiesStorageElement, Guid>(createDbContext, metricContext);

        [Injected]
        private readonly Func<SqlDbContext> createDbContext;

    }
}