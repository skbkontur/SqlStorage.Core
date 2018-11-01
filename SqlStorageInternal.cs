using System;
using System.Linq;
using System.Linq.Expressions;

using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;

using MoreLinq;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore
{
    internal class SqlStorageInternal<TEntry, TKey> : ISqlStorage<TEntry, TKey>
        where TEntry : class, ISqlEntity<TKey>
    {
        public SqlStorageInternal(Func<SqlDbContext> createDbContext, bool disposeContextOnOperationFinish)
        {
            this.createDbContext = createDbContext;
            this.disposeContextOnOperationFinish = disposeContextOnOperationFinish;
        }

        [CanBeNull]
        public TEntry TryRead([NotNull] TKey id)
        {
            return WithDbContext(context => context.Set<TEntry>().Find(id));
        }

        [NotNull, ItemNotNull]
        public TEntry[] TryRead([NotNull, ItemNotNull] TKey[] ids)
        {
            if (!ids.Any())
                return new TEntry[0];

            return WithDbContext(context => context.Set<TEntry>().AsNoTracking().Where(e => ids.Contains(e.Id)).ToArray());
        }

        [NotNull, ItemNotNull]
        public TEntry[] ReadAll()
        {
            return WithDbContext(context => context.Set<TEntry>().AsNoTracking().ToArray());
        }

        public void CreateOrUpdate([NotNull] TEntry entity, [CanBeNull] Expression<Func<TEntry, object>> onExpression = null, [CanBeNull] Expression<Func<TEntry, TEntry, TEntry>> whenMatched = null)
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

        public void CreateOrUpdate([NotNull, ItemNotNull] TEntry[] entities, [CanBeNull] Expression<Func<TEntry, object>> onExpression = null, [CanBeNull] Expression<Func<TEntry, TEntry, TEntry>> whenMatched = null)
        {
            if (!entities.Any())
                return;

            WithDbContext(context =>
                {
                    // Sql statement cannot have more than 65535 parameters, so we need to perform updates with limited entities count
                    entities.Batch(1000)
                            .ForEach(batch =>
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

        public void Delete([NotNull, ItemNotNull] TKey[] ids)
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

        public void Delete([NotNull] TKey id)
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

        public void Delete([NotNull] Expression<Func<TEntry, bool>> criterion)
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

        public TEntry[] Find([NotNull] Expression<Func<TEntry, bool>> criterion, int limit)
        {
            return WithDbContext(context => context.Set<TEntry>().AsNoTracking().Where(criterion).Take(limit).ToArray());
        }

        public TEntry[] Find<TOrderProp>([NotNull] Expression<Func<TEntry, bool>> criterion, [NotNull] Expression<Func<TEntry, TOrderProp>> orderBy, int limit)
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

        private readonly Func<SqlDbContext> createDbContext;
        private readonly bool disposeContextOnOperationFinish;
    }
}