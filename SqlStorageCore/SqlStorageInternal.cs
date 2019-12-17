using System;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using SkbKontur.SqlStorageCore.Exceptions;
using SkbKontur.SqlStorageCore.Linq;

namespace SkbKontur.SqlStorageCore
{
    internal class SqlStorageInternal : ISqlStorage
    {
        public SqlStorageInternal(Func<SqlDbContext> createDbContext, bool disposeContextOnOperationFinish)
        {
            this.createDbContext = createDbContext;
            this.disposeContextOnOperationFinish = disposeContextOnOperationFinish;
        }

        public TEntry? TryRead<TEntry, TKey>(TKey id)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull
        {
            return WithDbContext(context => context.Set<TEntry>().Find(id));
        }

        public TEntry[] TryRead<TEntry, TKey>(TKey[] ids)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull
        {
            if (!ids.Any())
                return new TEntry[0];

            return WithDbContext(context => context.Set<TEntry>().AsNoTracking().Where(e => ids.Contains(e.Id)).ToArray());
        }

        public TEntry[] ReadAll<TEntry, TKey>()
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull
        {
            return WithDbContext(context => context.Set<TEntry>().AsNoTracking().ToArray());
        }

        public void CreateOrUpdate<TEntry, TKey>(TEntry entity, Expression<Func<TEntry, object>>? onExpression = null, Expression<Func<TEntry, TEntry, TEntry>>? whenMatched = null)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull
        {
            try
            {
                WithDbContext(context =>
                    {
                        var upsertCommandBuilder = context.Upsert(entity).On(onExpression ?? (e => e.Id));
                        if (whenMatched != null)
                        {
                            upsertCommandBuilder = upsertCommandBuilder.WhenMatched(whenMatched);
                        }
                        upsertCommandBuilder.Run();
                    });
            }
            catch (PostgresException exception)
            {
                throw ToSqlStorageException(exception);
            }
        }

        public void CreateOrUpdate<TEntry, TKey>(TEntry[] entities, Expression<Func<TEntry, object>>? onExpression = null, Expression<Func<TEntry, TEntry, TEntry>>? whenMatched = null)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull
        {
            if (!entities.Any())
                return;
            try
            {
                WithDbContext(context =>
                    {
                        // Sql statement cannot have more than 65535 parameters, so we need to perform updates with limited entities count
                        entities.Batch(1000).ForEach(batch =>
                            {
                                var upsertCommandBuilder = context.UpsertRange(batch).On(onExpression ?? (e => e.Id));
                                if (whenMatched != null)
                                {
                                    upsertCommandBuilder = upsertCommandBuilder.WhenMatched(whenMatched);
                                }
                                upsertCommandBuilder.Run();
                            });
                    });
            }
            catch (PostgresException exception)
            {
                throw ToSqlStorageException(exception);
            }
        }

        public void Delete<TEntry, TKey>(TKey[] ids)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull
        {
            if (!ids.Any())
                return;

            WithDbContext(context =>
                {
                    var entities = context.Set<TEntry>().AsNoTracking().Where(e => ids.Contains(e.Id)).ToArray();
                    if (entities.Any())
                    {
                        context.Set<TEntry>().RemoveRange(entities);
                        context.SaveChanges();
                    }
                });
        }

        public void Delete<TEntry, TKey>(TKey id)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull
        {
            WithDbContext(context =>
                {
                    var entity = context.Set<TEntry>().Find(id);
                    if (entity != null)
                    {
                        context.Set<TEntry>().Remove(entity);
                        context.SaveChanges();
                    }
                });
        }

        public void Delete<TEntry, TKey>(Expression<Func<TEntry, bool>> criterion)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull
        {
            WithDbContext(context =>
                {
                    var entities = context.Set<TEntry>().AsNoTracking().Where(criterion).ToArray();
                    if (entities.Any())
                    {
                        context.Set<TEntry>().RemoveRange(entities);
                        context.SaveChanges();
                    }
                });
        }

        public TEntry[] Find<TEntry, TKey>(Expression<Func<TEntry, bool>> criterion, int limit)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull
        {
            return WithDbContext(context => context.Set<TEntry>().AsNoTracking().Where(criterion).Take(limit).ToArray());
        }

        public TEntry[] Find<TEntry, TKey, TOrderProp>(Expression<Func<TEntry, bool>> criterion, Expression<Func<TEntry, TOrderProp>> orderBy, int limit)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull
        {
            return WithDbContext(context => context.Set<TEntry>().AsNoTracking().Where(criterion).OrderBy(orderBy).Take(limit).ToArray());
        }

        private void WithDbContext(Action<SqlDbContext> action)
        {
            if (disposeContextOnOperationFinish)
            {
                using var context = createDbContext();
                action(context);
            }
            else
                action(createDbContext());
        }

        private TResult WithDbContext<TResult>(Func<SqlDbContext, TResult> func)
        {
            if (disposeContextOnOperationFinish)
            {
                using var context = createDbContext();
                return func(context);
            }
            return func(createDbContext());
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
    }
}