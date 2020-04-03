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
            await storage.CreateOrUpdateAsync(entity);
            await storage.TryReadAsync(entity.Id);

            metricEvent.Should().NotBeNull();
            metricEvent!.Tags.Count.Should().Be(1);
            metricEvent!.Tags.First().Value.Should().Be("Read.SingleEntry.Time");
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
            await storage.CreateOrUpdateAsync(entities);
            await storage.TryReadAsync(entities.Select(x => x.Id).ToArray());

            metricEvent.Should().NotBeNull();
            metricEvent!.Tags.Count.Should().Be(1);
            metricEvent!.Tags.First().Value.Should().Be("Read.Entries.Time");
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
            await storage.CreateOrUpdateAsync(entities);
            await storage.ReadAllAsync();

            metricEvent.Should().NotBeNull();
            metricEvent!.Tags.Count.Should().Be(1);
            metricEvent!.Tags.First().Value.Should().Be("Read.All.Time");
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
            await storage.CreateOrUpdateAsync(entity);
            await storage.DeleteAsync(entity.Id);

            metricEvent.Should().NotBeNull();
            metricEvent!.Tags.Count.Should().Be(1);
            metricEvent!.Tags.First().Value.Should().Be("Delete.SingleEntry.Time");
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
            await storage.TryReadAsync(entity.Id);

            metricEvent.Should().NotBeNull();
            metricEvent!.Tags.Count.Should().Be(1);
            metricEvent!.Tags.First().Value.Should().Be("Read.SingleEntry.Time");
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
            await storage.DeleteAsync(entity.Id);

            metricEvent.Should().NotBeNull();
            metricEvent!.Tags.Count.Should().Be(1);
            metricEvent!.Tags.First().Value.Should().Be("Delete.SingleEntry.Time");
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
            await storage.CreateOrUpdateAsync(entities);

            metricEvent.Should().NotBeNull();
            metricEvent!.Tags.Count.Should().Be(1);
            metricEvent!.Tags.First().Value.Should().Be("CreateOrUpdate.Entries.Time");
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
            await storage.CreateOrUpdateAsync(entities);
            await storage.FindAsync(e => e.StringProperty == stringProperty, entitiesCount);

            metricEvent.Should().NotBeNull();
            metricEvent!.Tags.Count.Should().Be(1);
            metricEvent!.Tags.First().Value.Should().Be("Find.ByCriterion.Time");
            metricEvent!.Unit.Should().Be("seconds");
            metricEvent!.Value.Should().BeLessThan(1);
        }

        private const string stringProperty = "d65dfy556";
        private const int entitiesCount = 1000;

        private TestValueTypedPropertiesStorageElement[] GetEntities() =>
            Enumerable.Range(0, entitiesCount).Select(x => GetEntity()).ToArray();

        private TestValueTypedPropertiesStorageElement GetEntity() =>
            new TestValueTypedPropertiesStorageElement {Id = Guid.NewGuid(), StringProperty = stringProperty};

        private ConcurrentSqlStorage<TestValueTypedPropertiesStorageElement, Guid> GetStorage(MetricContext metricContext) =>
            new ConcurrentSqlStorage<TestValueTypedPropertiesStorageElement, Guid>(createDbContext, metricContext);

        [Injected]
        private readonly Func<SqlDbContext> createDbContext;
    }
}