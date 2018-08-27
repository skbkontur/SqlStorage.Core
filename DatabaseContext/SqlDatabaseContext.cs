using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;
using SKBKontur.Catalogue.EDI.SqlStorageCore.Mapping;
using SKBKontur.Catalogue.EDI.SqlStorageCore.Migrations;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.DatabaseContext
{
    public sealed class SqlDatabaseContext : DbContext
    {
        public SqlDatabaseContext(SqlConnectionProvider connectionProvider, SqlEntitiesRegistry sqlEntitiesRegistry, SqlMigrationsAssemblyNameProvider sqlMigrationsAssemblyNameProvider)
        {
            this.sqlMigrationsAssemblyNameProvider = sqlMigrationsAssemblyNameProvider;
            this.sqlEntitiesRegistry = sqlEntitiesRegistry;
            connectionString = connectionProvider.ConnectionString;
        }

        protected override void OnConfiguring([NotNull] DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(connectionString, options =>
                {
                    options.EnableRetryOnFailure();
                    if (sqlMigrationsAssemblyNameProvider.IsMigrationsAssemblyNameDefined)
                        options.MigrationsAssembly(sqlMigrationsAssemblyNameProvider.MigrationsAssemblyName);
                });
            optionsBuilder.ReplaceService<IMigrationsSqlGenerator, SqlMigrationsScriptGenerator>();
            optionsBuilder.ReplaceService<IMigrationsAnnotationProvider, SqlMigrationsAnnotationProvider>();
        }

        protected override void OnModelCreating([NotNull] ModelBuilder modelBuilder)
        {
            ConfigureEventLog(modelBuilder);

            foreach (var type in sqlEntitiesRegistry.GetEntitesTypes())
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

            var logTypeBuilder = modelBuilder.Entity<EventLogStorageElement>();
            logTypeBuilder.Property(l => l.Id).HasDefaultValueSql("uuid_generate_v4()");
            logTypeBuilder.Property(l => l.Offset).UseNpgsqlSerialColumn();
            logTypeBuilder.HasIndex(l => new {l.Offset}).ForNpgsqlHasMethod("brin");

            foreach (var type in sqlEntitiesRegistry.GetEntitesTypes())
            {
                modelBuilder.Entity(type).HasEventLogWriteTrigger();
            }

            modelBuilder.Query<DateTimeStorageElement>();
        }

        [NotNull]
        private readonly string connectionString;

        [NotNull]
        private readonly SqlMigrationsAssemblyNameProvider sqlMigrationsAssemblyNameProvider;

        [NotNull]
        private readonly SqlEntitiesRegistry sqlEntitiesRegistry;
    }
}