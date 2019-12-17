using System;
using System.Linq;
using System.Linq.Expressions;

namespace SkbKontur.SqlStorageCore
{
    public static class SqlStorageExtensions
    {
        public static TEntity? FindSingleOrDefault<TEntity, TKey>(this IConcurrentSqlStorage<TEntity, TKey> storage, Expression<Func<TEntity, bool>> criterion)
            where TEntity : class, ISqlEntity<TKey>
            where TKey : notnull
        {
            var searchResult = storage.Find(criterion, 2);
            if (searchResult.Length <= 1)
                return searchResult.FirstOrDefault();

            throw new InvalidOperationException($"Found more than one {typeof(TEntity).Name} trying to get single.");
        }
    }
}