using System.Net;
using System.Reflection;

using GroboContainer.Infection;

using Newtonsoft.Json;

namespace SkbKontur.SqlStorageCore.Tests.TestUtils
{
    [IgnoredImplementation]
    public class TestSqlDbContextSettings : ISqlDbContextSettings
    {
        public TestSqlDbContextSettings(string database, SqlEntitiesRegistry sqlEntitiesRegistry, Assembly? migrationsAssembly)
        {
            Database = database;
            SqlEntitiesRegistry = sqlEntitiesRegistry;
            MigrationsAssembly = migrationsAssembly;
        }

        public string Host { get; } = IPAddress.Loopback.ToString();

        public int? Port => null;

        public string Username => "postgres";

        public string Password { get; } = "postgres";

        public string Database { get; }

        public int MaxRetryRequestOnFailureCount => 6;

        public SqlEntitiesRegistry SqlEntitiesRegistry { get; }

        public Assembly? MigrationsAssembly { get; }

        public JsonConverter[]? CustomJsonConverters { get; set; }
    }
}