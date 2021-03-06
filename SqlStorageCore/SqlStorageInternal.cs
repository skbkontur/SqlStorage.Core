using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using SkbKontur.SqlStorageCore.Exceptions;
using SkbKontur.SqlStorageCore.Linq;

using Vostok.Metrics;
using Vostok.Metrics.Primitives.Timer;

namespace SkbKontur.SqlStorageCore
{
    internal class SqlStorageInternal : ISqlStorage
    {
        public SqlStorageInternal(Func<SqlDbContext> createDbContext, IMetricContext? metricContext, bool disposeContextOnOperationFinish)
        {
            this.createDbContext = createDbContext;
            this.disposeContextOnOperationFinish = disposeContextOnOperationFinish;
            this.metricContext = metricContext;
        }

        public async Task<TEntry?> TryReadAsync<TEntry, TKey>(TKey id, CancellationToken cancellationToken = default)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull
        {
            return await WithDbContext(context => context.Set<TEntry>().FindAsync(new object[] {id}, cancellationToken).AsTask(), "Read.SingleEntry.Time");
        }

        public async Task<TEntry[]> TryReadAsync<TEntry, TKey>(TKey[] ids, CancellationToken cancellationToken = default)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull
        {
            if (!ids.Any())
                return new TEntry[0];

            return await WithDbContext(context => context.Set<TEntry>().AsNoTracking().Where(e => ids.Contains(e.Id)).ToArrayAsync(cancellationToken), "Read.Entries.Time");
        }

        public async Task<TEntry[]> ReadAllAsync<TEntry, TKey>(CancellationToken cancellationToken = default)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull
        {
            return await WithDbContext(context => context.Set<TEntry>().AsNoTracking().ToArrayAsync(cancellationToken), "Read.All.Time");
        }

        public async Task CreateOrUpdateAsync<TEntry, TKey>(TEntry entity,
                                                            Expression<Func<TEntry, object>>? onExpression = null,
                                                            Expression<Func<TEntry, TEntry, TEntry>>? whenMatched = null,
                                                            CancellationToken cancellationToken = default)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull
        {
            try
            {
                await WithDbContext(async context =>
                    {
                        var upsertCommandBuilder = context.Upsert(entity).On(onExpression ?? (e => e.Id));
                        if (whenMatched != null)
                        {
                            upsertCommandBuilder = upsertCommandBuilder.WhenMatched(whenMatched);
                        }
                        await upsertCommandBuilder.RunAsync(cancellationToken);
                    }, "CreateOrUpdate.Entry.Time");
            }
            catch (PostgresException exception)
            {
                throw ToSqlStorageException(exception);
            }
        }

        public async Task CreateOrUpdateAsync<TEntry, TKey>(TEntry[] entities,
                                                            Expression<Func<TEntry, object>>? onExpression = null,
                                                            Expression<Func<TEntry, TEntry, TEntry>>? whenMatched = null,
                                                            CancellationToken cancellationToken = default)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull
        {
            if (!entities.Any())
                return;
            try
            {
                await WithDbContext(async context =>
                    {
                        // Sql statement cannot have more than 65535 parameters, so we need to perform updates with limited entities count
                        foreach (var batch in entities.Batch(1000))
                        {
                            var upsertCommandBuilder = context.UpsertRange(batch).On(onExpression ?? (e => e.Id));
                            if (whenMatched != null)
                            {
                                upsertCommandBuilder = upsertCommandBuilder.WhenMatched(whenMatched);
                            }
                            await upsertCommandBuilder.RunAsync(cancellationToken);
                        }
                    }, "CreateOrUpdate.Entries.Time");
            }
            catch (PostgresException exception)
            {
                throw ToSqlStorageException(exception);
            }
        }

        public async Task DeleteAsync<TEntry, TKey>(TKey[] ids, CancellationToken cancellationToken = default)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull
        {
            if (!ids.Any())
                return;

            await WithDbContext(async context =>
                {
                    var entities = await context.Set<TEntry>().Where(e => ids.Contains(e.Id)).ToArrayAsync(cancellationToken);
                    if (entities.Any())
                    {
                        context.Set<TEntry>().RemoveRange(entities);
                        await context.SaveChangesAsync(cancellationToken);
                    }
                }, "Delete.Entries.Time");
        }

        public async Task DeleteAsync<TEntry, TKey>(TKey id, CancellationToken cancellationToken = default)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull
        {
            await WithDbContext(async context =>
                {
                    var entity = await context.Set<TEntry>().FindAsync(new object[] {id}, cancellationToken);
                    if (entity != null)
                    {
                        context.Set<TEntry>().Remove(entity);
                        await context.SaveChangesAsync(cancellationToken);
                    }
                }, "Delete.SingleEntry.Time");
        }

        public async Task DeleteAsync<TEntry, TKey>(Expression<Func<TEntry, bool>> criterion, CancellationToken cancellationToken = default)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull
        {
            await WithDbContext(async context =>
                {
                    var entities = await context.Set<TEntry>().Where(criterion).ToArrayAsync(cancellationToken);
                    if (entities.Any())
                    {
                        context.Set<TEntry>().RemoveRange(entities);
                        await context.SaveChangesAsync(cancellationToken);
                    }
                }, "Delete.ByCriterion.Time");
        }

        public async Task<TEntry[]> FindAsync<TEntry, TKey>(Expression<Func<TEntry, bool>> criterion, int limit, CancellationToken cancellationToken = default)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull
        {
            return await WithDbContext(context => context.Set<TEntry>().AsNoTracking().Where(criterion).Take(limit).ToArrayAsync(cancellationToken), "Find.ByCriterion.Time");
        }

        public async Task<TEntry[]> FindAsync<TEntry, TKey, TOrderProp>(Expression<Func<TEntry, bool>> criterion, Expression<Func<TEntry, TOrderProp>> orderBy, int limit, CancellationToken cancellationToken = default)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull
        {
            return await WithDbContext(context => context.Set<TEntry>().AsNoTracking().Where(criterion).OrderBy(orderBy).Take(limit).ToArrayAsync(cancellationToken), "Find.ByCriterionAndOrdered.Time");
        }

        private async Task WithDbContext(Func<SqlDbContext, Task> action, string timerName)
        {
            using (metricContext?.CreateTimer(timerName).Measure())
            {
                if (disposeContextOnOperationFinish)
                {
                    await using var context = createDbContext();
                    await action(context);
                }
                else
                {
                    await action(createDbContext());
                }
            }
        }

        private async Task<TResult> WithDbContext<TResult>(Func<SqlDbContext, Task<TResult>> func, string timerName)
        {
            using (metricContext?.CreateTimer(timerName).Measure())
            {
                if (disposeContextOnOperationFinish)
                {
                    await using var context = createDbContext();
                    return await func(context);
                }

                return await func(createDbContext());
            }
        }

        private static SqlStorageException ToSqlStorageException(PostgresException postgresException)
        {
            return PostgresExceptionRecognizer.TryRecognizeException(postgresException, out var sqlStorageRecognizedException)
                   && sqlStorageRecognizedException != null
                       ? sqlStorageRecognizedException
                       : new UnknownSqlStorageException(postgresException);
        }

        private readonly Func<SqlDbContext> createDbContext;
        private readonly bool disposeContextOnOperationFinish;
        private readonly IMetricContext? metricContext;
    }
}