using JetBrains.Annotations;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.DatabaseContext
{
    public class SqlConnectionProvider
    {
        public SqlConnectionProvider([NotNull] string connectionString)
        {
            ConnectionString = connectionString;
        }

        [NotNull]
        public string ConnectionString { get; }
    }
}