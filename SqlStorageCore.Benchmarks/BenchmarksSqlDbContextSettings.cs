using System.Net;
using System.Reflection;

using Newtonsoft.Json;

namespace SkbKontur.SqlStorageCore.Benchmarks
{
    public class BenchmarksSqlDbContextSettings : ISqlDbContextSettings
    {
        public string Host { get; } = IPAddress.Loopback.ToString();
        public int? Port { get; } = null;
        public string Username { get; } = "postgres";
        public string Password { get; } = "postgres";
        public string Database { get; } = "benchmarks";
        public int MaxRetryRequestOnFailureCount { get; } = 6;
        public SqlEntitiesRegistry SqlEntitiesRegistry { get; } = new BenchmarkSqlEntitiesRegistry();
        public Assembly? MigrationsAssembly => GetType().Assembly;
        public JsonConverter[]? CustomJsonConverters { get; } = null;
    }
}