using JetBrains.Annotations;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Migrations
{
    public class MigrationsAssemblyNameProvider
    {
        public MigrationsAssemblyNameProvider()
        {
            MigrationsAssemblyName = null;
        }

        [NotNull]
        public MigrationsAssemblyNameProvider WithAssemblyName([NotNull] string name)
        {
            MigrationsAssemblyName = name;
            return this;
        }

        public bool IsMigrationsAssemblyNameDefined => !string.IsNullOrEmpty(MigrationsAssemblyName);
        public string MigrationsAssemblyName { get; private set; }
    }
}