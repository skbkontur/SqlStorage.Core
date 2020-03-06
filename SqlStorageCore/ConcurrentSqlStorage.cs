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
            internalStorage = new SqlStorageInternal(createDbContext, null, disposeContextOnOperationFinish : true);
        }

        public Task<TEntry?> TryReadAsync(TKey id, CancellationToken cancellationToken = default)
        {
            return internalStorage.TryReadAsync<TEntry, TKey>(id, cancellationToken);
        }

        public Task<TEntry[]> TryReadAsync(TKey[] ids, CancellationToken cancellationToken = default)
        {
            return internalStorage.TryReadAsync<TEntry, TKey>(ids, cancellationToken);
        }

        public Task<TEntry[]> ReadAllAsync(CancellationToken cancellationToken = default)
        {
            return internalStorage.ReadAllAsync<TEntry, TKey>(cancellationToken);
        }

        public Task CreateOrUpdateAsync(TEntry entity, Expression<Func<TEntry, object>>? onExpression = null, Expression<Func<TEntry, TEntry, TEntry>>? whenMatched = null, CancellationToken cancellationToken = default)
        {
            return internalStorage.CreateOrUpdateAsync<TEntry, TKey>(entity, onExpression, whenMatched, cancellationToken);
        }

        public Task CreateOrUpdateAsync(TEntry[] entities, Expression<Func<TEntry, object>>? onExpression = null, Expression<Func<TEntry, TEntry, TEntry>>? whenMatched = null, CancellationToken cancellationToken = default)
        {
            return internalStorage.CreateOrUpdateAsync<TEntry, TKey>(entities, onExpression, whenMatched, cancellationToken);
        }

        public Task DeleteAsync(TKey[] ids, CancellationToken cancellationToken = default)
        {
            return !ids.Any() ? Task.CompletedTask : InTransactionAsync((storage, ct) => storage.DeleteAsync<TEntry, TKey>(ids, ct), IsolationLevel.Serializable, cancellationToken);
        }

        public Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
        {
            return InTransactionAsync((storage, ct) => storage.DeleteAsync<TEntry, TKey>(id, ct), IsolationLevel.Serializable, cancellationToken);
        }

        public Task DeleteAsync(Expression<Func<TEntry, bool>> criterion, CancellationToken cancellationToken = default)
        {
            return InTransactionAsync((storage, ct) => storage.DeleteAsync<TEntry, TKey>(criterion, ct), IsolationLevel.Serializable, cancellationToken);
        }

        public Task<TEntry[]> FindAsync(Expression<Func<TEntry, bool>> criterion, int limit, CancellationToken cancellationToken = default)
        {
            return internalStorage.FindAsync<TEntry, TKey>(criterion, limit, cancellationToken);
        }

        public Task<TEntry[]> FindAsync<TOrderProp>(Expression<Func<TEntry, bool>> criterion, Expression<Func<TEntry, TOrderProp>> orderBy, int limit, CancellationToken cancellationToken = default)
        {
            return internalStorage.FindAsync<TEntry, TKey, TOrderProp>(criterion, orderBy, limit, cancellationToken);
        }

        public Task BatchAsync(Func<ISqlStorage, CancellationToken, Task> batchAction, IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
        {
            return InTransactionAsync(batchAction, isolationLevel, cancellationToken);
        }

        private async Task InTransactionAsync(Func<ISqlStorage, CancellationToken, Task> operation, IsolationLevel isolationLevel, CancellationToken cancellationToken)
        {
            await using var context = createDbContext();

            async Task PerformOperation(CancellationToken ct)
            {
                await using var transaction = await context.Database.BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false);
                var storage = new SqlStorageInternal(() => context, disposeContextOnOperationFinish : false);
                await operation(storage, ct).ConfigureAwait(false);
                await transaction.CommitAsync(ct).ConfigureAwait(false);
            }

            await context.Database.CreateExecutionStrategy().ExecuteAsync(PerformOperation, cancellationToken).ConfigureAwait(false);
        }

        private readonly Func<SqlDbContext> createDbContext;
        private readonly SqlStorageInternal internalStorage;
    }
}