using System;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;

using SKBKontur.Catalogue.EDI.SqlStorageCore.Exceptions;
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
        public void TestUpsertOnUniqueConstraintThrows()
        {
            var entity1 = GenerateObjects().First();
            var entity2 = GenerateObjects().First();
            (entity2.SomeId1, entity2.SomeId2) = (entity1.SomeId1, entity1.SomeId2);

            Action creation = () => sqlStorage.CreateOrUpdate(new[] {entity1, entity2});

            creation.Should().Throw<UniqueViolationException>();
        }

        [Test]
        public void UpsertEntity_WithoutRequiredValue_Throws()
        {
            var entity = GenerateObjects().First();
            entity.RequiredValue = null;

            Action saving = () => sqlStorage.CreateOrUpdate(entity);
            saving.Should().Throw<UnknownSqlStorageException>();
        }

        [Test]
        public void TestUpsertOnCustomExpressionPrimaryKeyNotUpdated()
        {
            var entity = GenerateObjects().First();
            sqlStorage.CreateOrUpdate(entity);
            var oldId = entity.Id;
            entity.Id = Guid.NewGuid();
            entity.StringValue = Guid.NewGuid().ToString();
            sqlStorage.CreateOrUpdate(entity, e => new {e.SomeId1, e.SomeId2}, (db, ins) => new TestUpsertSqlEntry {Id = db.Id, SomeId1 = db.SomeId1, SomeId2 = db.SomeId2, StringValue = ins.StringValue});
            var actual = sqlStorage.ReadAll();
            actual.Length.Should().Be(1);
            var actualEntity = actual.First();
            actualEntity.Id.Should().Be(oldId);
            actualEntity.StringValue.Should().Be(entity.StringValue);
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