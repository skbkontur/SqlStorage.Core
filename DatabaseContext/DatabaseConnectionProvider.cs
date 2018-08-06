using JetBrains.Annotations;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.DatabaseContext
{
    public class DatabaseConnectionProvider
    {
        public DatabaseConnectionProvider([NotNull] string connectionString)
        {
            ConnectionString = connectionString;
        }

        [NotNull]
        public string ConnectionString { get; }
    }
}