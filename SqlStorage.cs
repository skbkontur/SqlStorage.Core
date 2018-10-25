using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using FlexLabs.EntityFrameworkCore.Upsert;

using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore
{
    [UsedImplicitly]
    public class SqlStorage<TEntry, TKey> : ISqlStorage<TEntry, TKey>
        where TEntry : class, ISqlEntity<TKey>
    {
        public SqlStorage(Func<SqlDbContext> createDbContext)
            : this(createDbContext, disposeContextOnOperationFinish : true)
        {
        }

        private SqlStorage(Func<SqlDbContext> createDbContext, bool disposeContextOnOperationFinish)
        {
            this.createDbContext = createDbContext;
            this.disposeContextOnOperationFinish = disposeContextOnOperationFinish;
        }

        [CanBeNull]
        public TEntry TryRead(TKey id)
        {
            return WithDbContext(context => context.Set<TEntry>().Find(id));
        }

        [NotNull, ItemNotNull]
        public TEntry[] TryRead([NotNull] TKey[] ids)
        {
            return WithDbContext(context => context.Set<TEntry>().AsNoTracking().Where(e => ids.Contains(e.Id)).ToArray());
        }

        [NotNull, ItemNotNull]
        public TEntry[] ReadAll()
        {
            return WithDbContext(context => context.Set<TEntry>().AsNoTracking().ToArray());
        }

        public void CreateOrUpdate([NotNull] TEntry entity, [CanBeNull] Expression<Func<TEntry, object>> onExpression = null)
        {
            WithDbContext(context =>
                {
                    var upsertCommandBuilder = context.Upsert(entity).On(onExpression ?? (e => e.Id)).UpdateExcludeColumns(e => e.Id);
                    upsertCommandBuilder.Run();
                });
        }

        public void CreateOrUpdate([NotNull, ItemNotNull] TEntry[] entities, [CanBeNull] Expression<Func<TEntry, object>> onExpression = null)
        {
            WithDbContext(context =>
                {
                    var upsertTasks = entities.Select(entity => context.Upsert(entity).On(onExpression ?? (e => e.Id)).UpdateExcludeColumns(e => e.Id).RunAsync()).ToArray();
                    Task.WaitAll(upsertTasks);
                });
        }

        public void Delete([NotNull] TKey[] ids)
        {
            InTransaction(context =>
                {
                    var entities = context.Set<TEntry>().AsNoTracking().Where(e => ids.Contains(e.Id)).ToArray();
                    if (entities.Any())
                    {
                        context.Set<TEntry>().RemoveRange(entities);
                        context.SaveChanges();
                    }
                }, IsolationLevel.Serializable);
        }

        public void Delete(TKey id)
        {
            InTransaction(context =>
                {
                    var entity = context.Set<TEntry>().Find(id);
                    if (entity != null)
                    {
                        context.Set<TEntry>().Remove(entity);
                        context.SaveChanges();
                    }
                }, IsolationLevel.Serializable);
        }

        public void Delete([NotNull] Expression<Func<TEntry, bool>> criterion)
        {
            InTransaction(context =>
                {
                    var entities = context.Set<TEntry>().AsNoTracking().Where(criterion).ToArray();
                    if (entities.Any())
                    {
                        context.Set<TEntry>().RemoveRange(entities);
                        context.SaveChanges();
                    }
                }, IsolationLevel.Serializable);
        }

        [NotNull, ItemNotNull]
        public TEntry[] Find([NotNull] Expression<Func<TEntry, bool>> criterion)
        {
            return WithDbContext(context => context.Set<TEntry>().AsNoTracking().Where(criterion).ToArray());
        }

        [NotNull, ItemNotNull]
        public TEntry[] Find<TOrderProp>([NotNull] Expression<Func<TEntry, bool>> criterion, [NotNull] Expression<Func<TEntry, TOrderProp>> orderBy, int limit)
        {
            return WithDbContext(context => context.Set<TEntry>().AsNoTracking().Where(criterion).OrderBy(orderBy).Take(limit).ToArray());
        }

        public void Batch([NotNull] Action<ISqlStorage<TEntry, TKey>> batchAction, IsolationLevel isolationLevel)
        {
            InTransaction(context =>
                {
                    var storage = new SqlStorage<TEntry, TKey>(() => context, disposeContextOnOperationFinish : false);
                    batchAction(storage);
                }, isolationLevel);
        }

        private void InTransaction([NotNull] Action<SqlDbContext> operation, IsolationLevel isolationLevel)
        {
            WithDbContext(context =>
                {
                    context.Database.CreateExecutionStrategy().Execute(() =>
                        {
                            WithDbContext(ctx =>
                                {
                                    using (var transaction = ctx.Database.BeginTransaction(isolationLevel))
                                    {
                                        operation(ctx);
                                        transaction.Commit();
                                    }
                                });
                        });
                });
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