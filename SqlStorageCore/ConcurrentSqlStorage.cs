using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace SkbKontur.SqlStorageCore
{
    public class ConcurrentSqlStorage<TEntry, TKey> : IConcurrentSqlStorage<TEntry, TKey>
        where TEntry : class, ISqlEntity<TKey>
        where TKey : notnull
    {
        public ConcurrentSqlStorage(Func<SqlDbContext> createDbContext)
        {
            this.createDbContext = createDbContext;
            internalStorage = new SqlStorageInternal(createDbContext, disposeContextOnOperationFinish : true);
        }

        public async Task<TEntry?> TryReadAsync(TKey id, CancellationToken cancellationToken = default)
        {
            return await internalStorage.TryReadAsync<TEntry, TKey>(id, cancellationToken);
        }

        public async Task<TEntry[]> TryReadAsync(TKey[] ids, CancellationToken cancellationToken = default)
        {
            return await internalStorage.TryReadAsync<TEntry, TKey>(ids, cancellationToken);
        }

        public async Task<TEntry[]> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            return await internalStorage.ReadAllAsync<TEntry, TKey>(cancellationToken);
        }

        public async Task CreateOrUpdateAsync(TEntry entity, Expression<Func<TEntry, object>>? onExpression = null, Expression<Func<TEntry, TEntry, TEntry>>? whenMatched = null, CancellationToken cancellationToken = default)
        {
            await internalStorage.CreateOrUpdateAsync<TEntry, TKey>(entity, onExpression, whenMatched, cancellationToken);
        }

        public async Task CreateOrUpdateAsync(TEntry[] entities, Expression<Func<TEntry, object>>? onExpression = null, Expression<Func<TEntry, TEntry, TEntry>>? whenMatched = null, CancellationToken cancellationToken = default)
        {
            await internalStorage.CreateOrUpdateAsync<TEntry, TKey>(entities, onExpression, whenMatched, cancellationToken);
        }

        public async Task DeleteAsync(TKey[] ids, CancellationToken cancellationToken = default)
        {
            if (!ids.Any())
                return;
            await InTransactionAsync(async storage => await storage.DeleteAsync<TEntry, TKey>(ids, cancellationToken), IsolationLevel.Serializable, cancellationToken);
        }

        public async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
        {
            await InTransactionAsync(async storage => await storage.DeleteAsync<TEntry, TKey>(id, cancellationToken), IsolationLevel.Serializable, cancellationToken);
        }

        public async Task DeleteAsync(Expression<Func<TEntry, bool>> criterion, CancellationToken cancellationToken = default)
        {
            await InTransactionAsync(storage => storage.DeleteAsync<TEntry, TKey>(criterion, cancellationToken), IsolationLevel.Serializable, cancellationToken);
        }

        public async Task<TEntry[]> FindAsync(Expression<Func<TEntry, bool>> criterion, int limit, CancellationToken cancellationToken = default)
        {
            return await internalStorage.FindAsync<TEntry, TKey>(criterion, limit, cancellationToken);
        }

        public async Task<TEntry[]> FindAsync<TOrderProp>(Expression<Func<TEntry, bool>> criterion, Expression<Func<TEntry, TOrderProp>> orderBy, int limit, CancellationToken cancellationToken = default)
        {
            return await internalStorage.FindAsync<TEntry, TKey, TOrderProp>(criterion, orderBy, limit, cancellationToken);
        }

        public async Task BatchAsync(Action<ISqlStorage> batchAction, IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
        {
            await InTransactionAsync(batchAction, isolationLevel, cancellationToken);
        }

        private async Task InTransactionAsync(Action<ISqlStorage> operation, IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
        {
            using (var context = createDbContext())
                await context.Database.CreateExecutionStrategy().ExecuteAsync( async cts =>   
                    {
                        using (var ctx = createDbContext())
                        using (var transaction =  await ctx.Database.BeginTransactionAsync(isolationLevel, cts))
                        {
                            var storage = new SqlStorageInternal(() => ctx, disposeContextOnOperationFinish : false);
                            operation(storage);
                            transaction.Commit();
                        }
                    }, cancellationToken);
        }

        private readonly Func<SqlDbContext> createDbContext;
        private readonly SqlStorageInternal internalStorage;
    }
}