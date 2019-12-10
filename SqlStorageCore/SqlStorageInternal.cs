using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using SkbKontur.SqlStorageCore.Exceptions;

namespace SkbKontur.SqlStorageCore
{
    internal class SqlStorageInternal : ISqlStorage
    {
        public SqlStorageInternal(Func<SqlDbContext> createDbContext, bool disposeContextOnOperationFinish)
        {
            this.createDbContext = createDbContext;
            this.disposeContextOnOperationFinish = disposeContextOnOperationFinish;
        }

        [CanBeNull]
        public TEntry TryRead<TEntry, TKey>([NotNull] TKey id)
            where TEntry : class, ISqlEntity<TKey>
        {
            return WithDbContext(context => context.Set<TEntry>().Find(id));
        }

        [NotNull, ItemNotNull]
        public TEntry[] TryRead<TEntry, TKey>([NotNull, ItemNotNull] TKey[] ids)
            where TEntry : class, ISqlEntity<TKey>
        {
            if (!ids.Any())
                return new TEntry[0];

            return WithDbContext(context => context.Set<TEntry>().AsNoTracking().Where(e => ids.Contains(e.Id)).ToArray());
        }

        [NotNull, ItemNotNull]
        public TEntry[] ReadAll<TEntry, TKey>()
            where TEntry : class, ISqlEntity<TKey>
        {
            return WithDbContext(context => context.Set<TEntry>().AsNoTracking().ToArray());
        }

        public void CreateOrUpdate<TEntry, TKey>([NotNull] TEntry entity, [CanBeNull] Expression<Func<TEntry, object>> onExpression = null, [CanBeNull] Expression<Func<TEntry, TEntry, TEntry>> whenMatched = null)
            where TEntry : class, ISqlEntity<TKey>
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

        public void CreateOrUpdate<TEntry, TKey>([NotNull, ItemNotNull] TEntry[] entities, [CanBeNull] Expression<Func<TEntry, object>> onExpression = null, [CanBeNull] Expression<Func<TEntry, TEntry, TEntry>> whenMatched = null)
            where TEntry : class, ISqlEntity<TKey>
        {
            if (!entities.Any())
                return;
            try
            {
                WithDbContext(context =>
                    {
                        // Sql statement cannot have more than 65535 parameters, so we need to perform updates with limited entities count
                        var batches = new List<IEnumerable<TEntry>>();
                        const int maxElementsInBatch = 1000;
                        var takenElements = 0;
                        while (takenElements < entities.Length) 
                        {
                            batches.Add(entities.Skip(takenElements).Take(maxElementsInBatch));
                            takenElements += maxElementsInBatch;
                        }

                        batches.ForEach(batch =>
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

        public void Delete<TEntry, TKey>([NotNull, ItemNotNull] TKey[] ids)
            where TEntry : class, ISqlEntity<TKey>
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

        public void Delete<TEntry, TKey>([NotNull] TKey id)
            where TEntry : class, ISqlEntity<TKey>
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

        public void Delete<TEntry, TKey>([NotNull] Expression<Func<TEntry, bool>> criterion)
            where TEntry : class, ISqlEntity<TKey>
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

        public TEntry[] Find<TEntry, TKey>([NotNull] Expression<Func<TEntry, bool>> criterion, int limit)
            where TEntry : class, ISqlEntity<TKey>
        {
            return WithDbContext(context => context.Set<TEntry>().AsNoTracking().Where(criterion).Take(limit).ToArray());
        }

        public TEntry[] Find<TEntry, TKey, TOrderProp>([NotNull] Expression<Func<TEntry, bool>> criterion, [NotNull] Expression<Func<TEntry, TOrderProp>> orderBy, int limit)
            where TEntry : class, ISqlEntity<TKey>
        {
            return WithDbContext(context => context.Set<TEntry>().AsNoTracking().Where(criterion).OrderBy(orderBy).Take(limit).ToArray());
        }

        private void WithDbContext([NotNull] Action<SqlDbContext> action)
        {
            if (disposeContextOnOperationFinish)
                using (var context = createDbContext())
                    action(context);
            else
                action(createDbContext());
        }

        private TResult WithDbContext<TResult>([NotNull] Func<SqlDbContext, TResult> func)
        {
            if (disposeContextOnOperationFinish)
                using (var context = createDbContext())
                    return func(context);
            return func(createDbContext());
        }

        [NotNull]
        private static SqlStorageException ToSqlStorageException([NotNull] PostgresException postgresException)
            => PostgresExceptionRecognizer.TryRecognizeException(postgresException, out var sqlStorageRecognizedException)
                   ? sqlStorageRecognizedException
                   : new UnknownSqlStorageException(postgresException);

        private readonly Func<SqlDbContext> createDbContext;
        private readonly bool disposeContextOnOperationFinish;
    }
}