using System;
using System.Collections.Generic;

using FluentAssertions;
using FluentAssertions.Equivalency;

using SKBKontur.Catalogue.Core.DataGeneration.Fluent;
using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;
using SKBKontur.Catalogue.EDI.SqlStorageCore.Storage;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestWrappers;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EntityStorageTests
{
    [EdiTestSuite, WithTestEntityStorage]
    public class EntityStorageTestBase<TEntity>
        where TEntity : class, IIdentifiableEntity, new()
    {
        protected readonly Func<EquivalencyAssertionOptions<TEntity>, EquivalencyAssertionOptions<TEntity>> equivalenceOptionsConfig = EquivalenceOptionsConfig;

        private static EquivalencyAssertionOptions<T> EquivalenceOptionsConfig<T>(EquivalencyAssertionOptions<T> options)
        {
            options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1)).WhenTypeIs<DateTime>();
            return options;
        }

        protected static IEnumerable<TEntity> GenerateObjects()
        {
            return DataGenerator.Random<TEntity>().WithGuidProperty(x => x.Id);
        }

        protected static void AssertUnorderedArraysEquality(IEnumerable<TEntity> actualObjects, IEnumerable<TEntity> objects)
        {
            actualObjects.Should().BeEquivalentTo(objects, options =>
                {
                    options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1)).WhenTypeIs<DateTime>();
                    return options;
                });
        }

        [Injected]
        protected readonly IEntityStorage<TEntity> entityStorage;
    }
}