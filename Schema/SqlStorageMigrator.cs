using System;
using System.Linq;

using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Schema
{
    [UsedImplicitly]
    public class SqlStorageMigrator
    {
        public SqlStorageMigrator(Func<SqlDbContext> createDbContext, ILoggerProvider logger)
        {
            this.createDbContext = createDbContext;
            this.logger = logger.CreateLogger($"{nameof(SqlStorageMigrator)}");
        }

        public void Migrate([CanBeNull] string migrationName = null)
        {
            using (var context = createDbContext())
            {
                var lastAppliedMigration = GetLastAppliedMigrationName(context);
                try
                {
                    if (migrationName == null)
                        context.Database.Migrate();
                    else
                        context.GetService<IMigrator>().Migrate(migrationName);
                }
                catch (Exception e)
                {
                    var justAppliedMigration = GetLastAppliedMigrationName(context);
                    logger.LogCritical($"Database migration failed. Last applied migration: {justAppliedMigration}. Exception: {e}");

                    if (justAppliedMigration != lastAppliedMigration)
                    {
                        logger.LogInformation($"Some migrations were applied. Last just applied migration: {justAppliedMigration}. Starting rollback...");
                        try
                        {
                            context.GetService<IMigrator>().Migrate(lastAppliedMigration);
                        }
                        catch (Exception rollbackException)
                        {
                            logger.LogCritical($"Rollback to {lastAppliedMigration} failed. Exception: {rollbackException}");
                            throw;
                        }
                    }

                    throw;
                }
            }
        }

        [CanBeNull]
        private static string GetLastAppliedMigrationName(SqlDbContext context)
        {
            var appliedMigrations = context.Database.GetAppliedMigrations().ToList();
            return appliedMigrations.OrderBy(m => m).LastOrDefault();
        }

        private readonly Func<SqlDbContext> createDbContext;
        private readonly ILogger logger;
    }
}