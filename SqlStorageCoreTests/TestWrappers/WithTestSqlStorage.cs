using System.Reflection;

using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using Microsoft.Extensions.Logging;

using SkbKontur.SqlStorageCore.Schema;
using SkbKontur.SqlStorageCore.Tests.TestUtils;

using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;
using Vostok.Logging.Microsoft;

using LogLevel = Vostok.Logging.Abstractions.LogLevel;

namespace SkbKontur.SqlStorageCore.Tests.TestWrappers
{
    public class WithTestSqlStorage : GroboTestSuiteWrapperAttribute
    {
        public override void SetUp(string suiteName, Assembly testAssembly, IEditableGroboTestContext suiteContext)
        {
            var container = suiteContext.Container;
            var consoleLog = new ConsoleLog();
            container.Configurator.ForAbstraction<ILog>().UseInstances(consoleLog);
            var loggerFactory = new LoggerFactory(new[] {new VostokLoggerProvider(consoleLog.WithMinimumLevel(LogLevel.Warn))});
            container.Configurator.ForAbstraction<ILoggerFactory>().UseInstances(loggerFactory);
            container.Configurator.ForAbstraction<ISqlDbContextSettings>().UseInstances(new TestSqlDbContextSettings(DbName, TestSqlEntitiesRegistry, MigrationsAssembly));
            container.Get<SqlStorageMigrator>().MigrateAsync().GetAwaiter().GetResult();
        }

        public const string DbName = "Tests";

        public static readonly TestSqlEntitiesRegistry TestSqlEntitiesRegistry = new TestSqlEntitiesRegistry();
        public static readonly Assembly MigrationsAssembly = typeof(WithTestSqlStorage).Assembly;
    }
}