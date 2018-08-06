using System;
using System.Linq;
using System.Linq.Expressions;

using NUnit.Framework;

using SKBKontur.Catalogue.EDIFunctionalTests.Commons.TestWrappers;
using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EntityStorageTests.TestEntities;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EntityStorageTests
{
    [AndEntityStorageCleanUp(typeof(TestValueTypedPropertiesStorageElement))]
    public class DeleteTests : EntityStorageTestBase<TestValueTypedPropertiesStorageElement>
    {
        [Test]
        public void DeleteByCriterion()
        {
            var entities = GenerateObjects().Take(10).ToArray();
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

            entityStorage.CreateOrUpdate(entities);
            Expression<Func<TestValueTypedPropertiesStorageElement, bool>> expression = e => e.IntProperty == intConstraint && !(e.BoolProperty ?? true);
            entityStorage.Delete(expression);
            var actual = entityStorage.ReadAll();
            var shouldDeleted = entities.Where(expression.Compile()).Select(x => x.Id).ToArray();
            AssertUnorderedArraysEquality(actual, entities.Where(e => !shouldDeleted.Contains(e.Id)));
        }
    }
}