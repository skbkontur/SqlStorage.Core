using System;
using System.Linq.Expressions;

using JetBrains.Annotations;

namespace SkbKontur.SqlStorageCore
{
    public interface ISqlStorage
    {
        TEntry? TryRead<TEntry, TKey>(TKey id)
            where TEntry : class, ISqlEntity<TKey>;

        TEntry[] TryRead<TEntry, TKey>(TKey[] ids)
            where TEntry : class, ISqlEntity<TKey>;

        TEntry[] ReadAll<TEntry, TKey>()
            where TEntry : class, ISqlEntity<TKey>;

        void CreateOrUpdate<TEntry, TKey>(TEntry entity, Expression<Func<TEntry, object>>? onExpression = null, Expression<Func<TEntry, TEntry, TEntry>>? whenMatched = null)
            where TEntry : class, ISqlEntity<TKey>;

        void CreateOrUpdate<TEntry, TKey>(TEntry[] entities, Expression<Func<TEntry, object>>? onExpression = null, Expression<Func<TEntry, TEntry, TEntry>>? whenMatched = null)
            where TEntry : class, ISqlEntity<TKey>;

        void Delete<TEntry, TKey>(TKey[] ids)
            where TEntry : class, ISqlEntity<TKey>;

        void Delete<TEntry, TKey>(TKey id)
            where TEntry : class, ISqlEntity<TKey>;

        void Delete<TEntry, TKey>(Expression<Func<TEntry, bool>> criterion)
            where TEntry : class, ISqlEntity<TKey>;

        TEntry[] Find<TEntry, TKey>(Expression<Func<TEntry, bool>> criterion, int limit)
            where TEntry : class, ISqlEntity<TKey>;

        TEntry[] Find<TEntry, TKey, TOrderProp>(Expression<Func<TEntry, bool>> criterion, Expression<Func<TEntry, TOrderProp>> orderBy, int limit)
            where TEntry : class, ISqlEntity<TKey>;
    }
}