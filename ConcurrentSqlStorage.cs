using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore
{
    [UsedImplicitly]
    public class ConcurrentSqlStorage<TEntry, TKey> : IConcurrentSqlStorage<TEntry, TKey>
        where TEntry : class, ISqlEntity<TKey>
    {
        public ConcurrentSqlStorage(Func<SqlDbContext> createDbContext)
        {
            this.createDbContext = createDbContext;
            internalStorage = new SqlStorageInternal(createDbContext, disposeContextOnOperationFinish : true);
        }

        [CanBeNull]
        public TEntry TryRead([NotNull] TKey id)
        {
            return internalStorage.TryRead<TEntry, TKey>(id);
        }

        [NotNull, ItemNotNull]
        public TEntry[] TryRead([NotNull] TKey[] ids)
        {
            return internalStorage.TryRead<TEntry, TKey>(ids);
        }

        [NotNull, ItemNotNull]
        public TEntry[] ReadAll()
        {
            return internalStorage.ReadAll<TEntry, TKey>();
        }

        public void CreateOrUpdate([NotNull] TEntry entity, [CanBeNull] Expression<Func<TEntry, object>> onExpression = null, [CanBeNull] Expression<Func<TEntry, TEntry, TEntry>> whenMatched = null)
        {
            internalStorage.CreateOrUpdate<TEntry, TKey>(entity, onExpression, whenMatched);
        }

        public void CreateOrUpdate([NotNull, ItemNotNull] TEntry[] entities, [CanBeNull] Expression<Func<TEntry, object>> onExpression = null, [CanBeNull] Expression<Func<TEntry, TEntry, TEntry>> whenMatched = null)
        {
            internalStorage.CreateOrUpdate<TEntry, TKey>(entities, onExpression, whenMatched);
        }

        public void Delete([NotNull] TKey[] ids)
        {
            if (!ids.Any())
                return;
            InTransaction(storage => storage.Delete<TEntry, TKey>(ids), IsolationLevel.Serializable);
        }

        public void Delete([NotNull] TKey id)
        {
            InTransaction(storage => storage.Delete<TEntry, TKey>(id), IsolationLevel.Serializable);
        }

        public void Delete([NotNull] Expression<Func<TEntry, bool>> criterion)
        {
            InTransaction(storage => storage.Delete<TEntry, TKey>(criterion), IsolationLevel.Serializable);
        }

        [NotNull, ItemNotNull]
        public TEntry[] Find([NotNull] Expression<Func<TEntry, bool>> criterion, int limit)
        {
            return internalStorage.Find<TEntry, TKey>(criterion, limit);
        }

        [NotNull, ItemNotNull]
        public TEntry[] Find<TOrderProp>([NotNull] Expression<Func<TEntry, bool>> criterion, [NotNull] Expression<Func<TEntry, TOrderProp>> orderBy, int limit)
        {
            return internalStorage.Find<TEntry, TKey, TOrderProp>(criterion, orderBy, limit);
        }

        public void Batch([NotNull] Action<ISqlStorage> batchAction, IsolationLevel isolationLevel)
        {
            InTransaction(batchAction, isolationLevel);
        }

        private void InTransaction([NotNull] Action<ISqlStorage> operation, IsolationLevel isolationLevel)
        {
            using (var context = createDbContext())
                context.Database.CreateExecutionStrategy().Execute(() =>
                    {
                        using (var ctx = createDbContext())
                        using (var transaction = ctx.Database.BeginTransaction(isolationLevel))
                        {
                            var storage = new SqlStorageInternal(() => ctx, disposeContextOnOperationFinish : false);
                            operation(storage);
                            transaction.Commit();
                        }
                    });
        }

        private readonly Func<SqlDbContext> createDbContext;
        private readonly SqlStorageInternal internalStorage;
    }
}