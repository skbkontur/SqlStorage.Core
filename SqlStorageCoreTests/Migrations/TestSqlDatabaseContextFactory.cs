using GroboContainer.Core;
using GroboContainer.Impl;

using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;

using SkbKontur.SqlStorageCore.Tests.TestUtils;
using SkbKontur.SqlStorageCore.Tests.TestWrappers;

using Vostok.Logging.Console;
using Vostok.Logging.Microsoft;

namespace SkbKontur.SqlStorageCore.Tests.Migrations
{
    /// <inheritdoc />
    /// <summary>
    ///     Factory used by EntityFrameworkCore.Tools to create migrations
    /// </summary>
    public class TestSqlDatabaseContextFactory : IDesignTimeDbContextFactory<SqlDbContext>
    {
        public SqlDbContext CreateDbContext(string[] args)
        {
            var container = new Container(new ContainerConfiguration(AssembliesLoader.Load(), nameof(TestSqlDatabaseContextFactory), ContainerMode.UseShortLog));
            var vostokLoggerProvider = new VostokLoggerProvider(new ConsoleLog());
            container.Configurator.ForAbstraction<ILoggerFactory>().UseInstances(new LoggerFactory(new[] { vostokLoggerProvider }));
            var sqlDbContextSettings = new TestSqlDbContextSettings(WithTestSqlStorage.DbName, WithTestSqlStorage.TestSqlEntitiesRegistry, WithTestSqlStorage.MigrationsAssembly);
            container.Configurator.ForAbstraction<ISqlDbContextSettings>().UseInstances(sqlDbContextSettings);
            return container.Get<SqlDbContext>();
        }
    }
}