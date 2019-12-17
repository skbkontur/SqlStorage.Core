using System;
using System.Collections.Generic;

using AutoFixture;

using FluentAssertions;
using FluentAssertions.Equivalency;

using GroboContainer.NUnitExtensions;

using SkbKontur.SqlStorageCore.Tests.TestWrappers;

namespace SkbKontur.SqlStorageCore.Tests
{
    [GroboTestSuite, WithTestSqlStorage]
    public abstract class SqlStorageTestBase<TEntity, TKey>
        where TEntity : class, ISqlEntity<TKey>, new()
    {
        private static EquivalencyAssertionOptions<T> EquivalenceOptionsConfig<T>(EquivalencyAssertionOptions<T> options)
        {
            return options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1)).WhenTypeIs<DateTime>();
        }

        protected IEnumerable<TEntity> GenerateObjects(int count = 1)
        {
            return fixture.Build<TEntity>().CreateMany(count);
        }

        protected static void AssertUnorderedArraysEquality<T>(IEnumerable<T> actualObjects, IEnumerable<T> objects)
        {
            actualObjects.Should().BeEquivalentTo(objects, EquivalenceOptionsConfig);
        }

        protected readonly Func<EquivalencyAssertionOptions<TEntity>, EquivalencyAssertionOptions<TEntity>> equivalenceOptionsConfig = EquivalenceOptionsConfig;

        [Injected]
        protected readonly IConcurrentSqlStorage<TEntity, TKey> sqlStorage;

        private readonly Fixture fixture = new Fixture();
    }
}