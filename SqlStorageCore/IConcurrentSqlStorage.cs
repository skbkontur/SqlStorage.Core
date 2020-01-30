using System;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SkbKontur.SqlStorageCore
{
    public interface IConcurrentSqlStorage<TEntry, in TKey>
        where TEntry : class, ISqlEntity<TKey>
        where TKey : notnull
    {
        Task<TEntry?> TryReadAsync(TKey id, CancellationToken cancellationToken = default);

        Task<TEntry[]> TryReadAsync(TKey[] ids, CancellationToken cancellationToken = default);

        Task<TEntry[]> ReadAllAsync(CancellationToken cancellationToken = default);

        Task CreateOrUpdateAsync(TEntry entity, Expression<Func<TEntry, object>>? onExpression = null, Expression<Func<TEntry, TEntry, TEntry>>? whenMatched = null, CancellationToken cancellationToken = default);
        Task CreateOrUpdateAsync(TEntry[] entities, Expression<Func<TEntry, object>>? onExpression = null, Expression<Func<TEntry, TEntry, TEntry>>? whenMatched = null, CancellationToken cancellationToken = default);

        Task DeleteAsync(TKey[] ids, CancellationToken cancellationToken = default);
        Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
        Task DeleteAsync(Expression<Func<TEntry, bool>> criterion, CancellationToken cancellationToken = default);

        Task<TEntry[]> FindAsync(Expression<Func<TEntry, bool>> criterion, int limit, CancellationToken cancellationToken = default);

        Task<TEntry[]> FindAsync<TOrderProp>(Expression<Func<TEntry, bool>> criterion, Expression<Func<TEntry, TOrderProp>> orderBy, int limit, CancellationToken cancellationToken = default);

        Task BatchAsync(Func<ISqlStorage, Task> batchAction, IsolationLevel isolationLevel, CancellationToken cancellationToken = default);
    }
}