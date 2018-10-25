using System;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;

using SKBKontur.Catalogue.EDIFunctionalTests.Commons.TestWrappers;
using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests
{
    [AndSqlStorageCleanUp(typeof(TestUpsertSqlEntry))]
    public class TestUpsert : SqlStorageTestBase<TestUpsertSqlEntry, Guid>
    {
        [Test]
        public void TestUpsertOnPrimaryKey()
        {
            var entity1 = GenerateObjects().First();
            var entity2 = GenerateObjects().First();
            sqlStorage.CreateOrUpdate(new[] {entity1, entity2});
            entity1.StringValue = Guid.NewGuid().ToString();
            sqlStorage.CreateOrUpdate(entity1);
            var actual = sqlStorage.ReadAll();
            actual.Length.Should().Be(2);
            AssertUnorderedArraysEquality(actual, new[] {entity1, entity2});
        }

        [Test]
        public void TestUpsertOnCustomExpressionMatchExists()
        {
            var entity1 = GenerateObjects().First();
            var entity2 = GenerateObjects().First();
            sqlStorage.CreateOrUpdate(new[] {entity1, entity2});
            entity1.StringValue = Guid.NewGuid().ToString();
            sqlStorage.CreateOrUpdate(entity1, e => new {e.SomeId1, e.SomeId2});
            var actual = sqlStorage.ReadAll();
            actual.Length.Should().Be(2);
            AssertUnorderedArraysEquality(actual, new[] {entity1, entity2});
        }

        [Test]
        public void TestUpsertOnCustomExpressionPrimaryKeyNotUpdated()
        {
            var entity = GenerateObjects().First();
            sqlStorage.CreateOrUpdate(entity);
            var oldId = entity.Id;
            entity.Id = Guid.NewGuid();
            entity.StringValue = Guid.NewGuid().ToString();
            sqlStorage.CreateOrUpdate(entity, e => new {e.SomeId1, e.SomeId2});
            var actual = sqlStorage.ReadAll();
            actual.Length.Should().Be(1);
            actual.First().Id.Should().Be(oldId);
        }

        [Test]
        public void TestUpsertOnCustomExpressionMatchNotExists()
        {
            var entity1 = GenerateObjects().First();
            sqlStorage.CreateOrUpdate(entity1);
            var entity2 = GenerateObjects().First();
            entity2.SomeId1 = entity1.SomeId1;

            sqlStorage.CreateOrUpdate(entity2, e => new {e.SomeId1, e.SomeId2});

            var actual = sqlStorage.ReadAll();
            actual.Length.Should().Be(2);
            AssertUnorderedArraysEquality(actual, new[] {entity1, entity2});
        }
    }
}