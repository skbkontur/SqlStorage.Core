using System.Reflection;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore
{
    public interface ISqlDbContextSettings
    {
        [NotNull]
        string Host { get; }

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