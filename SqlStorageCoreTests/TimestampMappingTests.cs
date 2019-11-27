﻿using System;

using FluentAssertions;

using NUnit.Framework;

using SKBKontur.Catalogue.EDIFunctionalTests.Commons.TestWrappers;
using SKBKontur.Catalogue.Objects;
using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests
{
    [AndSqlStorageCleanUp(typeof(TestTimestampElement))]
    public class TimestampMappingTests : SqlStorageTestBase<TestTimestampElement, Guid>
    {
        [Test]
        public void TestReadWrite()
        {
            var entity = new TestTimestampElement {Id = Guid.NewGuid(), Timestamp = new Timestamp(new DateTime(2018, 07, 01, 0, 0, 0, DateTimeKind.Utc))};
            sqlStorage.CreateOrUpdate(entity);
            var actual = sqlStorage.TryRead(entity.Id);
            actual.Should().BeEquivalentTo(entity);
        }
    }
}