using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SkbKontur.SqlStorageCore
{
    public interface ISqlStorage
    {
        Task<TEntry?> TryReadAsync<TEntry, TKey>(TKey id, CancellationToken cancellationToken = default)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull;

        Task<TEntry[]> TryReadAsync<TEntry, TKey>(TKey[] ids, CancellationToken cancellationToken = default)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull;

        Task<TEntry[]> ReadAllAsync<TEntry, TKey>(CancellationToken cancellationToken = default)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull;

        Task CreateOrUpdateAsync<TEntry, TKey>(TEntry entity, Expression<Func<TEntry, object>>? onExpression = null, Expression<Func<TEntry, TEntry, TEntry>>? whenMatched = null, CancellationToken cancellationToken = default)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull;

        Task CreateOrUpdateAsync<TEntry, TKey>(TEntry[] entities, Expression<Func<TEntry, object>>? onExpression = null, Expression<Func<TEntry, TEntry, TEntry>>? whenMatched = null, CancellationToken cancellationToken = default)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull;

        Task DeleteAsync<TEntry, TKey>(TKey[] ids, CancellationToken cancellationToken = default)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull;

        Task DeleteAsync<TEntry, TKey>(TKey id, CancellationToken cancellationToken = default)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull;

        Task DeleteAsync<TEntry, TKey>(Expression<Func<TEntry, bool>> criterion, CancellationToken cancellationToken = default)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull;

        Task<TEntry[]> FindAsync<TEntry, TKey>(Expression<Func<TEntry, bool>> criterion, int limit, CancellationToken cancellationToken = default)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull;

        Task<TEntry[]> FindAsync<TEntry, TKey, TOrderProp>(Expression<Func<TEntry, bool>> criterion, Expression<Func<TEntry, TOrderProp>> orderBy, int limit, CancellationToken cancellationToken = default)
            where TEntry : class, ISqlEntity<TKey>
            where TKey : notnull;
    }
}