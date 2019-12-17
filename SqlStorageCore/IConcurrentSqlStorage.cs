using System;
using System.Data;
using System.Linq.Expressions;

namespace SkbKontur.SqlStorageCore
{
    public interface IConcurrentSqlStorage<TEntry, in TKey>
        where TEntry : class, ISqlEntity<TKey>
    {
        TEntry? TryRead(TKey id);

        TEntry[] TryRead(TKey[] ids);

        TEntry[] ReadAll();

        void CreateOrUpdate(TEntry entity, Expression<Func<TEntry, object>>? onExpression = null, Expression<Func<TEntry, TEntry, TEntry>>? whenMatched = null);
        void CreateOrUpdate(TEntry[] entities, Expression<Func<TEntry, object>>? onExpression = null, Expression<Func<TEntry, TEntry, TEntry>>? whenMatched = null);

        void Delete(TKey[] ids);
        void Delete(TKey id);
        void Delete(Expression<Func<TEntry, bool>> criterion);

        TEntry[] Find(Expression<Func<TEntry, bool>> criterion, int limit);

        TEntry[] Find<TOrderProp>(Expression<Func<TEntry, bool>> criterion, Expression<Func<TEntry, TOrderProp>> orderBy, int limit);

        void Batch(Action<ISqlStorage> batchAction, IsolationLevel isolationLevel);
    }
}