using GroboContainer.Core;
using GroboContainer.Impl;

using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore.Design;

using SKBKontur.Catalogue.EDI.SqlStorageCore;
using SKBKontur.Catalogue.EDIFunctionalTests.Commons.TestWrappers;
using SKBKontur.Catalogue.ServiceLib;
using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestWrappers;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.Migrations
{
    /// <inheritdoc />
    /// <summary>
    ///     Factory used by EntityFrameworkCore.Tools to create migrations
    /// </summary>
    [UsedImplicitly]
    public class TestSqlDatabaseContextFactory : IDesignTimeDbContextFactory<SqlDbContext>
    {
        [NotNull]
        public SqlDbContext CreateDbContext(string[] args)
        {
            var container = new Container(new ContainerConfiguration(AssembliesLoader.Load(), nameof(TestSqlDatabaseContextFactory), ContainerMode.UseShortLog));
            var sqlDbContextSettings = new WithSqlStorage.TestSqlDbContextSettings(WithTestSqlStorage.DbName, WithTestSqlStorage.TestSqlEntitiesRegistry, WithTestSqlStorage.MigrationsAssembly);
            container.Configurator.ForAbstraction<ISqlDbContextSettings>().UseInstances(sqlDbContextSettings);
            return container.Get<SqlDbContext>();
        }
    }
}