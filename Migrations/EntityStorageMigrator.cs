using System;

using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;

using SKBKontur.Catalogue.EDI.SqlStorageCore.DatabaseContext;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Migrations
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EntityStorageMigrator
    {
        public EntityStorageMigrator(Func<EntitiesDatabaseContext> createDbContext)
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

        private readonly Func<EntitiesDatabaseContext> createDbContext;
    }
}