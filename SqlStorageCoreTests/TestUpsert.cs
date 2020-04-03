using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using NUnit.Framework;

using SkbKontur.SqlStorageCore.Exceptions;
using SkbKontur.SqlStorageCore.Tests.TestEntities;
using SkbKontur.SqlStorageCore.Tests.TestWrappers;

namespace SkbKontur.SqlStorageCore.Tests
{
    [AndSqlStorageCleanUp(typeof(TestUpsertSqlEntry))]
    public class TestUpsert : SqlStorageTestBase<TestUpsertSqlEntry, Guid>
    {
        [Test]
        public async Task TestUpsertOnPrimaryKey()
        {
            var entity1 = GenerateObjects().First();
            var entity2 = GenerateObjects().First();
            await sqlStorage.CreateOrUpdateAsync(new[] {entity1, entity2});
            entity1.StringValue = Guid.NewGuid().ToString();
            await sqlStorage.CreateOrUpdateAsync(entity1);
            var actual = await sqlStorage.ReadAllAsync();
            actual.Length.Should().Be(2);
            AssertUnorderedArraysEquality(actual, new[] {entity1, entity2});
        }

        [Test]
        public async Task TestUpsertOnCustomExpressionMatchExists()
        {
            var entity1 = GenerateObjects().First();
            var entity2 = GenerateObjects().First();
            await sqlStorage.CreateOrUpdateAsync(new[] {entity1, entity2});
            entity1.StringValue = Guid.NewGuid().ToString();
            await sqlStorage.CreateOrUpdateAsync(entity1, e => new {e.SomeId1, e.SomeId2});
            var actual = await sqlStorage.ReadAllAsync();
            actual.Length.Should().Be(2);
            AssertUnorderedArraysEquality(actual, new[] {entity1, entity2});
        }

        [Test]
        public async Task CreateEntity_WithDuplicateUniqueConstraint_Throws()
        {
            var entity1 = GenerateObjects().First();
            var entity2 = GenerateObjects().First();
            (entity2.SomeId1, entity2.SomeId2) = (entity1.SomeId1, entity1.SomeId2);

            Func<Task> creation = async () => await sqlStorage.CreateOrUpdateAsync(new[] {entity1, entity2});
            await creation.Should().ThrowAsync<UniqueViolationException>();
        }

        [Test]
        public async Task UpdateEntity_WithDuplicateUniqueConstraint_Throws()
        {
            var entity1 = GenerateObjects().First();
            var entity2 = GenerateObjects().First();
            await sqlStorage.CreateOrUpdateAsync(new[] {entity1, entity2});

            (entity2.SomeId1, entity2.SomeId2) = (entity1.SomeId1, entity1.SomeId2);
            Func<Task> updating = async () => await sqlStorage.CreateOrUpdateAsync(entity2);

            await updating.Should().ThrowAsync<UniqueViolationException>();
        }

        [Test]
        public async Task CreateEntity_WithoutRequiredValue_Throws()
        {
            var entity = GenerateObjects().First();
            entity.RequiredValue = null;

            Func<Task> creating = async () => await sqlStorage.CreateOrUpdateAsync(entity);
            (await creating.Should().ThrowAsync<NotNullViolationException>())
                .Which.ColumnName
                .Should().Be(nameof(entity.RequiredValue));
        }

        [Test]
        public async Task UpdateEntity_WithoutRequiredValue_Throws()
        {
            var entity = GenerateObjects().First();
            await sqlStorage.CreateOrUpdateAsync(entity);

            entity.RequiredValue = null;
            Func<Task> updating = async () => await sqlStorage.CreateOrUpdateAsync(entity);
            (await updating.Should().ThrowAsync<NotNullViolationException>())
                .Which.ColumnName
                .Should().Be(nameof(entity.RequiredValue));
        }

        [Test]
        public async Task TestUpsertOnCustomExpressionPrimaryKeyNotUpdated()
        {
            var entity = GenerateObjects().First();
            await sqlStorage.CreateOrUpdateAsync(entity);
            var oldId = entity.Id;
            entity.Id = Guid.NewGuid();
            entity.StringValue = Guid.NewGuid().ToString();
            await sqlStorage.CreateOrUpdateAsync(entity, e => new {e.SomeId1, e.SomeId2}, (db, ins) => new TestUpsertSqlEntry {Id = db.Id, SomeId1 = db.SomeId1, SomeId2 = db.SomeId2, StringValue = ins.StringValue});
            var actual = await sqlStorage.ReadAllAsync();
            actual.Length.Should().Be(1);
            var actualEntity = actual.First();
            actualEntity.Id.Should().Be(oldId);
            actualEntity.StringValue.Should().Be(entity.StringValue);
        }

        [Test]
        public async Task TestUpsertOnCustomExpressionMatchNotExists()
        {
            var entity1 = GenerateObjects().First();
            await sqlStorage.CreateOrUpdateAsync(entity1);
            var entity2 = GenerateObjects().First();
            entity2.SomeId1 = entity1.SomeId1;

            await sqlStorage.CreateOrUpdateAsync(entity2, e => new {e.SomeId1, e.SomeId2});

            var actual = await sqlStorage.ReadAllAsync();
            actual.Length.Should().Be(2);
            AssertUnorderedArraysEquality(actual, new[] {entity1, entity2});
        }
    }
}