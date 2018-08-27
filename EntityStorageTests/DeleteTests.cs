using System;
using System.Linq;
using System.Linq.Expressions;

using NUnit.Framework;

using SKBKontur.Catalogue.EDIFunctionalTests.Commons.TestWrappers;
using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EntityStorageTests.TestEntities;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EntityStorageTests
{
    [AndSqlStorageCleanUp(typeof(TestValueTypedPropertiesStorageElement))]
    public class DeleteTests : EntityStorageTestBase<TestValueTypedPropertiesStorageElement>
    {
        [Test]
        public void DeleteByCriterion()
        {
            var entities = GenerateObjects(count: 10).ToArray();
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

            sqlStorage.CreateOrUpdate(entities);
            Expression<Func<TestValueTypedPropertiesStorageElement, bool>> expression = e => e.IntProperty == intConstraint && !(e.BoolProperty ?? true);
            sqlStorage.Delete(expression);
            var actual = sqlStorage.ReadAll();
            var shouldDeleted = entities.Where(expression.Compile()).Select(x => x.Id).ToArray();
            AssertUnorderedArraysEquality(actual, entities.Where(e => !shouldDeleted.Contains(e.Id)));
        }
    }
}