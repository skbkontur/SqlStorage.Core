using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;

using GroboContainer.NUnitExtensions;

using NUnit.Framework;

using SkbKontur.SqlStorageCore.Tests.TestUtils;

using Vostok.Metrics;
using Vostok.Metrics.Models;
using Vostok.Metrics.Senders;

namespace SkbKontur.SqlStorageCore.Tests
{
    public class SqlStorageInternalMetricsTests
    {
        [Test]
        public async Task TestReadTimer()
        {
            MetricEvent? metricEvent = null;
            var metricContext = new MetricContext(new MetricContextConfig(new AdHocMetricEventSender(e => metricEvent = e)));

            var storage = new SqlStorageInternal(createDbContext, metricContext, true);

            var entity = new SimpleTestEntity(5);
            storage.CreateOrUpdate<SimpleTestEntity, int>(entity);
            storage.TryRead<SimpleTestEntity, int>(entity.Id);

            metricEvent.Should().NotBeNull();
            metricEvent!.Value.Should().BeLessThan(1);
        }

        [Injected]
        private Func<SqlDbContext> createDbContext;

        private class SimpleTestEntity : ISqlEntity<int>
        {
            public int Id { get; set; }
            public SimpleTestEntity(int id) => Id = id;
        }
    }
}
