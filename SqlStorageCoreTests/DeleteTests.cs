using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using FluentAssertions;

using NUnit.Framework;

using SkbKontur.SqlStorageCore.Tests.TestEntities;
using SkbKontur.SqlStorageCore.Tests.TestWrappers;

namespace SkbKontur.SqlStorageCore.Tests
{
    [AndSqlStorageCleanUp(typeof(TestValueTypedPropertiesStorageElement))]
    public class DeleteTests : SqlStorageTestBase<TestValueTypedPropertiesStorageElement, Guid>
    {
        [Test]
        public async Task DeleteByCriterion()
        {
            var entities = GenerateObjects(count : 10).ToArray();
            const int intConstraint = -42;
            for (var i = 0; i < entities.Length / 2; i++)
            {
                entities[i].IntProperty = intConstraint;
                entities[i].BoolProperty = false;
            }
            for (var i = entities.Length / 2; i < entities.Length; i++)
            {
                entities[i].IntProperty = 7;
            }

            await sqlStorage.CreateOrUpdateAsync(entities);
            Expression<Func<TestValueTypedPropertiesStorageElement, bool>> expression = e => e.IntProperty == intConstraint && !(e.BoolProperty ?? true);
            await sqlStorage.DeleteAsync(expression);
            var actual = await sqlStorage.ReadAllAsync();
            var shouldDeleted = entities.Where(expression.Compile()).Select(x => x.Id).ToArray();
            AssertUnorderedArraysEquality(actual, entities.Where(e => !shouldDeleted.Contains(e.Id)));
        }

        [Test]
        public async Task DeleteNonExisting()
        {
            Action nonexistentDeletion = () => sqlStorage.DeleteAsync(Guid.NewGuid());
            Func<Task> func = () => sqlStorage.DeleteAsync(Guid.NewGuid());
            await func.Should().NotThrowAsync();
            nonexistentDeletion.Should().NotThrow();
        }
    }
}