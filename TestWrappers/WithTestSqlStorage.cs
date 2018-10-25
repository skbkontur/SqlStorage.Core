using System.Reflection;

using SKBKontur.Catalogue.EDIFunctionalTests.Commons.TestWrappers;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestWrappers
{
    public class WithTestSqlStorage : WithSqlStorage
    {
        public WithTestSqlStorage()
            : base(DbName, TestSqlEntitiesRegistry, MigrationsAssembly)
        {
        }

        public static readonly TestSqlEntitiesRegistry TestSqlEntitiesRegistry = new TestSqlEntitiesRegistry();
        public const string DbName = "Tests";
        public static readonly Assembly MigrationsAssembly = typeof(WithTestSqlStorage).Assembly;
    }
}