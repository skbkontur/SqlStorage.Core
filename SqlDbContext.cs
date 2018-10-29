﻿using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql;

using SKBKontur.Catalogue.EDI.SqlStorageCore.EventLog;
using SKBKontur.Catalogue.EDI.SqlStorageCore.Schema;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore
{
    public sealed class SqlDbContext : DbContext
    {
        public SqlDbContext([NotNull] ISqlDbContextSettings settings)
        {
            this.settings = settings;
        }

        protected override void OnConfiguring([NotNull] DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = new NpgsqlConnectionStringBuilder
                {
                    Host = settings.Host,
                    Username = settings.Username,
                    Password = settings.Password,
                    Database = settings.Database,
                }.ToString();
            optionsBuilder.UseNpgsql(connectionString, options =>
                {
                    options.EnableRetryOnFailure(settings.MaxRetryRequestOnFailureCount);
                    if (settings.MigrationsAssembly != null)
                        options.MigrationsAssembly(settings.MigrationsAssembly.FullName);
                });
            optionsBuilder.ReplaceService<IMigrationsSqlGenerator, SqlMigrationsScriptGenerator>();
            optionsBuilder.ReplaceService<IMigrationsAnnotationProvider, SqlMigrationsAnnotationProvider>();
        }

        protected override void OnModelCreating([NotNull] ModelBuilder modelBuilder)
        {
            ConfigureEventLog(modelBuilder);

            foreach (var type in settings.SqlEntitiesRegistry.GetEntityTypes())
            {
                modelBuilder
                    .Entity(type)
                    .ApplyTimestampConverter()
                    .ApplyJsonColumns()
                    .ApplyIndices()
                    .ApplyUniqueConstraints();
            }
        }

        private void ConfigureEventLog([NotNull] ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("uuid-ossp");

            var logTypeBuilder = modelBuilder.Entity<SqlEventLogEntry>();
            logTypeBuilder.ApplyTimestampConverter();

            logTypeBuilder.HasIndex(l => new {l.Offset}).ForNpgsqlHasMethod("brin");
            logTypeBuilder.Property(l => l.Id).HasDefaultValueSql("uuid_generate_v4()");
            logTypeBuilder.Property(l => l.Offset).UseNpgsqlSerialColumn();
            logTypeBuilder.Property(l => l.EntityContent).HasColumnType("json");

            foreach (var type in settings.SqlEntitiesRegistry.GetEntityTypes())
                modelBuilder.Entity(type).HasEventLogWriteTrigger();
        }

        private readonly ISqlDbContextSettings settings;
    }
}