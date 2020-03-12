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
            await using var context = createDbContext();
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

        private async Task WaitDatabaseAvailable(DbContext context)
        {
            var attempts = 0;
            var dbConnection = context.Database.GetDbConnection();
            while (attempts < 6)
            {
                try
                {
                    await dbConnection.OpenAsync().ConfigureAwait(false);
                    dbConnection.Close();
                    return;
                }
                catch (SocketException e)
                {
                    logger.Debug(e, $"Unable to connect to database on attempt {attempts}");
                }
                catch (DbException)
                {
                    return; //tcp connection established, exception is sql-related
                }
                attempts++;
                await Task.Delay(TimeSpan.FromSeconds(attempts * 10));
            }

            var connectionString = new NpgsqlConnectionStringBuilder(dbConnection.ConnectionString);
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