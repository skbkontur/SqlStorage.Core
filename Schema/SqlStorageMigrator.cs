using System;

using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Schema
{
    [UsedImplicitly]
    public class SqlStorageMigrator
    {
        public SqlStorageMigrator(Func<SqlDbContext> createDbContext)
        {
            this.createDbContext = createDbContext;
        }

        public void Migrate()
        {
            using (var context = createDbContext())
                context.Database.Migrate();
        }

        private readonly Func<SqlDbContext> createDbContext;
    }
}