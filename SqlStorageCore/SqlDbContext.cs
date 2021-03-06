using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;

using Npgsql;

using SkbKontur.SqlStorageCore.EventLog;
using SkbKontur.SqlStorageCore.Json;
using SkbKontur.SqlStorageCore.Schema;

namespace SkbKontur.SqlStorageCore
{
    public sealed class SqlDbContext : DbContext
    {
        public SqlDbContext(ISqlDbContextSettings settings, ILoggerFactory loggerFactory)
        {
            this.settings = settings;
            this.loggerFactory = loggerFactory;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = new NpgsqlConnectionStringBuilder
                {
                    Host = settings.Host,
                    Username = settings.Username,
                    Password = settings.Password,
                    Database = settings.Database,
                };

            if (settings.Port.HasValue)
                connectionString.Port = settings.Port.Value;

            optionsBuilder.UseNpgsql(connectionString.ToString(), options =>
                {
                    options.EnableRetryOnFailure(settings.MaxRetryRequestOnFailureCount);
                    if (settings.MigrationsAssembly != null)
                        options.MigrationsAssembly(settings.MigrationsAssembly.FullName);
                });
            optionsBuilder.ReplaceService<IMigrationsSqlGenerator, SqlMigrationsScriptGenerator>();
            optionsBuilder.ReplaceService<IMigrationsAnnotationProvider, SqlMigrationsAnnotationProvider>();
            optionsBuilder.UseLoggerFactory(loggerFactory);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureEventLog(modelBuilder);
            var jsonConverters = CustomJsonConvertersBuilder.Build(settings.CustomJsonConverters);
            foreach (var type in settings.SqlEntitiesRegistry.GetEntityTypes())
            {
                modelBuilder
                    .Entity(type)
                    .ApplyTimestampConverter()
                    .ApplyJsonColumns(jsonConverters)
                    .ApplyIndices()
                    .ApplyUniqueConstraints();
            }
        }

        private void ConfigureEventLog(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("uuid-ossp");
            modelBuilder.HasDbFunction(() => PostgresFunctions.CurrentTransactionIdsSnapshot());
            modelBuilder.HasDbFunction(() => PostgresFunctions.SnapshotMinimalTransactionId(default));

            var logTypeBuilder = modelBuilder.Entity<SqlEventLogEntry>();
            logTypeBuilder.ApplyTimestampConverter();

            logTypeBuilder.HasIndex(l => new {l.Offset}).HasMethod("brin");
            logTypeBuilder.Property(l => l.Id).HasDefaultValueSql("uuid_generate_v4()");
            logTypeBuilder.Property(l => l.Offset).UseSerialColumn();
            logTypeBuilder.Property(l => l.EntityContent).HasColumnType("json");

            foreach (var type in settings.SqlEntitiesRegistry.GetEntityTypes())
                modelBuilder.Entity(type).HasEventLogWriteTrigger();
        }

        private readonly ISqlDbContextSettings settings;
        private readonly ILoggerFactory loggerFactory;
    }
}