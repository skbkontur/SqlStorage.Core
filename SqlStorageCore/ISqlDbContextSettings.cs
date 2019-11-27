using System.Reflection;

using JetBrains.Annotations;

namespace SkbKontur.SqlStorageCore
{
    public interface ISqlDbContextSettings
    {
        [NotNull]
        string Host { get; }

        int? Port { get; }

        [NotNull]
        string Username { get; }

        [NotNull]
        string Password { get; }

        [NotNull]
        string Database { get; }

        int MaxRetryRequestOnFailureCount { get; }

        [NotNull]
        SqlEntitiesRegistry SqlEntitiesRegistry { get; }

        [CanBeNull]
        Assembly MigrationsAssembly { get; }
    }
}