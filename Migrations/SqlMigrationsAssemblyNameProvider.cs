using JetBrains.Annotations;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Migrations
{
    public class SqlMigrationsAssemblyNameProvider
    {
        public SqlMigrationsAssemblyNameProvider()
        {
            MigrationsAssemblyName = null;
        }

        [NotNull]
        public SqlMigrationsAssemblyNameProvider WithAssemblyName([NotNull] string name)
        {
            MigrationsAssemblyName = name;
            return this;
        }

        public bool IsMigrationsAssemblyNameDefined => !string.IsNullOrEmpty(MigrationsAssemblyName);
        public string MigrationsAssemblyName { get; private set; }
    }
}