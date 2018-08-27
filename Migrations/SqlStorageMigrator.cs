using System;

using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;

using SKBKontur.Catalogue.EDI.SqlStorageCore.DatabaseContext;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Migrations
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class SqlStorageMigrator
    {
        public SqlStorageMigrator(Func<SqlDatabaseContext> createDbContext)
        {
            this.createDbContext = createDbContext;
        }

        public void Migrate()
        {
            using (var context = createDbContext())
            {
                context.Database.Migrate();
            }
        }

        private readonly Func<SqlDatabaseContext> createDbContext;
    }
}