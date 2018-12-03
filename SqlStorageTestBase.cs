using System;
using System.Collections.Generic;

using AutoFixture;

using FluentAssertions;
using FluentAssertions.Equivalency;

using GroboContainer.NUnitExtensions;

using SKBKontur.Catalogue.EDI.SqlStorageCore;
using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestWrappers;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests
{
    [GroboTestSuite, WithTestSqlStorage]
    public abstract class SqlStorageTestBase<TEntity, TKey>
        where TEntity : class, ISqlEntity<TKey>, new()
    {
        protected readonly Func<EquivalencyAssertionOptions<TEntity>, EquivalencyAssertionOptions<TEntity>> equivalenceOptionsConfig = EquivalenceOptionsConfig;

        private static EquivalencyAssertionOptions<T> EquivalenceOptionsConfig<T>(EquivalencyAssertionOptions<T> options)
        {
            options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1)).WhenTypeIs<DateTime>();
            return options;
        }

        protected IEnumerable<TEntity> GenerateObjects(int count = 1)
        {
            return fixture.Build<TEntity>().CreateMany(count);
        }

        protected static void AssertUnorderedArraysEquality<T>(IEnumerable<T> actualObjects, IEnumerable<T> objects)
        {
            actualObjects.Should().BeEquivalentTo(objects, options =>
                {
                    options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1)).WhenTypeIs<DateTime>();
                    return options;
                });
        }

        [Injected]
        protected readonly IConcurrentSqlStorage<TEntity, TKey> sqlStorage;

        private readonly Fixture fixture = new Fixture();
    }
}