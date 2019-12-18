using System;
using System.Data.Common;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql;

using SkbKontur.SqlStorageCore.Exceptions;

using Vostok.Logging.Abstractions;

namespace SkbKontur.SqlStorageCore.Schema
{
    public class SqlStorageMigrator
    {
        public SqlStorageMigrator(Func<SqlDbContext> createDbContext, ILog logger)
        {
            this.createDbContext = createDbContext;
            this.logger = logger.ForContext("SqlStorage.Migrator");
        }

        public async Task MigrateAsync(string? migrationName = null)
        {
            using var context = createDbContext();
            await WaitDatabaseAvailable(context);
            var lastAppliedMigration = await GetLastAppliedMigrationName(context);
            try
            {
                if (migrationName == null)
                    await context.Database.MigrateAsync();
                else
                    await context.GetService<IMigrator>().MigrateAsync(migrationName);
            }
            catch (Exception e)
            {
                var justAppliedMigration = await GetLastAppliedMigrationName(context);
                logger.Fatal($"Database migration failed. Last applied migration: {justAppliedMigration}. Exception: {e}");

                if (justAppliedMigration != lastAppliedMigration)
                {
                    logger.Info($"Some migrations were applied. Last just applied migration: {justAppliedMigration}. Starting rollback...");
                    try
                    {
                        await context.GetService<IMigrator>().MigrateAsync(lastAppliedMigration);
                    }
                    catch (Exception rollbackException)
                    {
                        logger.Fatal($"Rollback to {lastAppliedMigration} failed. Exception: {rollbackException}");
                        throw;
                    }
                }

                throw;
            }
        }

        private static async Task WaitDatabaseAvailable(DbContext context)
        {
            var attempts = 0;
            while (attempts < 6)
            {
                var canConnect = false;
                try
                {
                    canConnect = await context.Database.CanConnectAsync();
                }
                catch (SocketException)
                {
                    // could not establish connection, should retry
                }
                if (canConnect)
                    return;
                attempts++;
                await Task.Delay(TimeSpan.FromSeconds(attempts * 10));
            }

            var connectionString = new NpgsqlConnectionStringBuilder(context.Database.GetDbConnection().ConnectionString);
            throw new DatabaseUnavailableException(connectionString.Database, connectionString.Host, connectionString.Port);
        }

        private static async Task<string?> GetLastAppliedMigrationName(DbContext context)
        {
            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
            return appliedMigrations.OrderBy(m => m).LastOrDefault();
        }

        private readonly Func<SqlDbContext> createDbContext;
        private readonly ILog logger;
    }
}