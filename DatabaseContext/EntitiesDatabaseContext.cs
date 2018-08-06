using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;
using SKBKontur.Catalogue.EDI.SqlStorageCore.Mapping;
using SKBKontur.Catalogue.EDI.SqlStorageCore.Migrations;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.DatabaseContext
{
    public sealed class EntitiesDatabaseContext : DbContext
    {
        public EntitiesDatabaseContext(DatabaseConnectionProvider connectionProvider, EntitiesRegistry entitiesRegistry, MigrationsAssemblyNameProvider migrationsAssemblyNameProvider)
        {
            this.migrationsAssemblyNameProvider = migrationsAssemblyNameProvider;
            this.entitiesRegistry = entitiesRegistry;
            connectionString = connectionProvider.ConnectionString;
        }

        protected override void OnConfiguring([NotNull] DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(connectionString, options =>
                {
                    options.EnableRetryOnFailure();
                    if (migrationsAssemblyNameProvider.IsMigrationsAssemblyNameDefined)
                        options.MigrationsAssembly(migrationsAssemblyNameProvider.MigrationsAssemblyName);
                });
            optionsBuilder.ReplaceService<IMigrationsSqlGenerator, EntitiesMigrationsSqlGenerator>();
            optionsBuilder.ReplaceService<IMigrationsAnnotationProvider, EntitiesMigrationsAnnotationProvider>();
        }

        protected override void OnModelCreating([NotNull] ModelBuilder modelBuilder)
        {
            ConfigureEventLog(modelBuilder);

            foreach (var type in entitiesRegistry.GetEntitesTypes())
            {
                modelBuilder
                    .Entity(type)
                    .ApplyTimestampConverter()
                    .ApplyJsonColumns()
                    .ApplyIndices();
            }
        }

        private void ConfigureEventLog([NotNull] ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("uuid-ossp");

            var logTypeBuilder = modelBuilder.Entity<EventLogEntity>();
            logTypeBuilder.Property(l => l.Id).HasDefaultValueSql("uuid_generate_v4()");
            logTypeBuilder.Property(l => l.Offset).UseNpgsqlSerialColumn();
            logTypeBuilder.HasIndex(l => new {l.Offset}).ForNpgsqlHasMethod("brin");

            foreach (var type in entitiesRegistry.GetEntitesTypes())
            {
                modelBuilder.Entity(type).HasEventLogWriteTrigger();
            }

            modelBuilder.Query<DateTimeEntity>();
        }

        [NotNull]
        private readonly string connectionString;

        [NotNull]
        private readonly MigrationsAssemblyNameProvider migrationsAssemblyNameProvider;

        [NotNull]
        private readonly EntitiesRegistry entitiesRegistry;
    }
}