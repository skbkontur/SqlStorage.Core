using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SkbKontur.SqlStorageCore
{
    public static class SqlStorageExtensions
    {
        public static async Task<TEntity?>FindSingleOrDefaultAsync<TEntity, TKey>(this IConcurrentSqlStorage<TEntity, TKey> storage, Expression<Func<TEntity, bool>> criterion, CancellationToken cancellationToken = default)
            where TEntity : class, ISqlEntity<TKey>
            where TKey : notnull
        {
            var searchResult = await storage.FindAsync(criterion, 2, cancellationToken);
            if (searchResult.Length <= 1)
                return searchResult.FirstOrDefault();

            throw new InvalidOperationException($"Found more than one {typeof(TEntity).Name} trying to get single.");
        }
    }
}