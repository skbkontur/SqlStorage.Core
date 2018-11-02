using System;
using System.Linq.Expressions;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore
{
    public interface ISqlStorage
    {
        [CanBeNull]
        TEntry TryRead<TEntry, TKey>(TKey id)
            where TEntry : class, ISqlEntity<TKey>;

        [NotNull, ItemNotNull]
        TEntry[] TryRead<TEntry, TKey>([NotNull] TKey[] ids)
            where TEntry : class, ISqlEntity<TKey>;

        [NotNull, ItemNotNull]
        TEntry[] ReadAll<TEntry, TKey>()
            where TEntry : class, ISqlEntity<TKey>;

        void CreateOrUpdate<TEntry, TKey>([NotNull] TEntry entity, [CanBeNull] Expression<Func<TEntry, object>> onExpression = null, [CanBeNull] Expression<Func<TEntry, TEntry, TEntry>> whenMatched = null)
            where TEntry : class, ISqlEntity<TKey>;

        void CreateOrUpdate<TEntry, TKey>([NotNull, ItemNotNull] TEntry[] entities, [CanBeNull] Expression<Func<TEntry, object>> onExpression = null, [CanBeNull] Expression<Func<TEntry, TEntry, TEntry>> whenMatched = null)
            where TEntry : class, ISqlEntity<TKey>;

        void Delete<TEntry, TKey>([NotNull] TKey[] ids)
            where TEntry : class, ISqlEntity<TKey>;

        void Delete<TEntry, TKey>(TKey id)
            where TEntry : class, ISqlEntity<TKey>;

        void Delete<TEntry, TKey>([NotNull] Expression<Func<TEntry, bool>> criterion)
            where TEntry : class, ISqlEntity<TKey>;

        [NotNull, ItemNotNull]
        TEntry[] Find<TEntry, TKey>([NotNull] Expression<Func<TEntry, bool>> criterion, int limit)
            where TEntry : class, ISqlEntity<TKey>;

        [NotNull, ItemNotNull]
        TEntry[] Find<TEntry, TKey, TOrderProp>([NotNull] Expression<Func<TEntry, bool>> criterion, Expression<Func<TEntry, TOrderProp>> orderBy, int limit)
            where TEntry : class, ISqlEntity<TKey>;
    }
}