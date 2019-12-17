using System.Reflection;

namespace SkbKontur.SqlStorageCore
{
    public interface ISqlDbContextSettings
    {
        string Host { get; }

        int? Port { get; }

        string Username { get; }

        string Password { get; }

        string Database { get; }

        int MaxRetryRequestOnFailureCount { get; }

        SqlEntitiesRegistry SqlEntitiesRegistry { get; }

        Assembly? MigrationsAssembly { get; }
    }
}