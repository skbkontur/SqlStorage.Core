using System;
using System.Linq;
using System.Linq.Expressions;

using JetBrains.Annotations;

namespace SkbKontur.SqlStorageCore
{
    public static class SqlStorageExtensions
    {
        public static T? FindSingleOrDefault<T, TKey>(this IConcurrentSqlStorage<T, TKey> storage, Expression<Func<T, bool>> criterion)
            where T : class, ISqlEntity<TKey>
        {
            var searchResult = storage.Find(criterion, 2);
            if (searchResult.Length <= 1)
                return searchResult.FirstOrDefault();

            throw new InvalidOperationException($"Found more than one {typeof(T).Name} trying to get single.");
        }
    }
}