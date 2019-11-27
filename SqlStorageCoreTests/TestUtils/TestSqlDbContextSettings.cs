using System.Net;
using System.Reflection;

using GroboContainer.Infection;

using JetBrains.Annotations;

namespace SkbKontur.SqlStorageCore.Tests.TestUtils
{
    [IgnoredImplementation]
    public class TestSqlDbContextSettings : ISqlDbContextSettings
    {
        public TestSqlDbContextSettings([NotNull] string database, [NotNull] SqlEntitiesRegistry sqlEntitiesRegistry, [CanBeNull] Assembly migrationsAssembly)
        {
            Database = database;
            SqlEntitiesRegistry = sqlEntitiesRegistry;
            MigrationsAssembly = migrationsAssembly;
        }

        [NotNull]
        public string Host { get; } = IPAddress.Loopback.ToString();

        public int? Port { get; } = null;

        [NotNull]
        public string Username { get; } = "postgres";

        [NotNull]
        public string Password { get; } = "postgres";

        [NotNull]
        public string Database { get; }

        public int MaxRetryRequestOnFailureCount { get; } = 6;

        [NotNull]
        public SqlEntitiesRegistry SqlEntitiesRegistry { get; }

        [CanBeNull]
        public Assembly MigrationsAssembly { get; }
    }
}