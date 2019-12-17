using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

namespace SkbKontur.SqlStorageCore
{
    public class ConcurrentSqlStorage<TEntry, TKey> : IConcurrentSqlStorage<TEntry, TKey>
        where TEntry : class, ISqlEntity<TKey>
    {
        public ConcurrentSqlStorage(Func<SqlDbContext> createDbContext)
        {
            this.createDbContext = createDbContext;
            internalStorage = new SqlStorageInternal(createDbContext, disposeContextOnOperationFinish : true);
        }

        public TEntry? TryRead(TKey id)
        {
            return internalStorage.TryRead<TEntry, TKey>(id);
        }

        public TEntry[] TryRead(TKey[] ids)
        {
            return internalStorage.TryRead<TEntry, TKey>(ids);
        }

        public TEntry[] ReadAll()
        {
            return internalStorage.ReadAll<TEntry, TKey>();
        }

        public void CreateOrUpdate(TEntry entity, Expression<Func<TEntry, object>>? onExpression = null, Expression<Func<TEntry, TEntry, TEntry>>? whenMatched = null)
        {
            internalStorage.CreateOrUpdate<TEntry, TKey>(entity, onExpression, whenMatched);
        }

        public void CreateOrUpdate(TEntry[] entities, Expression<Func<TEntry, object>>? onExpression = null, Expression<Func<TEntry, TEntry, TEntry>>? whenMatched = null)
        {
            internalStorage.CreateOrUpdate<TEntry, TKey>(entities, onExpression, whenMatched);
        }

        public void Delete(TKey[] ids)
        {
            if (!ids.Any())
                return;
            InTransaction(storage => storage.Delete<TEntry, TKey>(ids), IsolationLevel.Serializable);
        }

        public void Delete(TKey id)
        {
            InTransaction(storage => storage.Delete<TEntry, TKey>(id), IsolationLevel.Serializable);
        }

        public void Delete(Expression<Func<TEntry, bool>> criterion)
        {
            InTransaction(storage => storage.Delete<TEntry, TKey>(criterion), IsolationLevel.Serializable);
        }

        public TEntry[] Find(Expression<Func<TEntry, bool>> criterion, int limit)
        {
            return internalStorage.Find<TEntry, TKey>(criterion, limit);
        }

        public TEntry[] Find<TOrderProp>(Expression<Func<TEntry, bool>> criterion, Expression<Func<TEntry, TOrderProp>> orderBy, int limit)
        {
            return internalStorage.Find<TEntry, TKey, TOrderProp>(criterion, orderBy, limit);
        }

        public void Batch(Action<ISqlStorage> batchAction, IsolationLevel isolationLevel)
        {
            InTransaction(batchAction, isolationLevel);
        }

        private void InTransaction(Action<ISqlStorage> operation, IsolationLevel isolationLevel)
        {
            using var context = createDbContext();
            context.Database.CreateExecutionStrategy().Execute(() =>
                {
                    using var ctx = createDbContext();
                    using var transaction = ctx.Database.BeginTransaction(isolationLevel);
                    var storage = new SqlStorageInternal(() => ctx, disposeContextOnOperationFinish : false);
                    operation(storage);
                    transaction.Commit();
                });
        }

        private readonly Func<SqlDbContext> createDbContext;
        private readonly SqlStorageInternal internalStorage;
    }
}