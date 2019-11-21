﻿using System;
using System.Data;
using System.Linq.Expressions;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore
{
    public interface IConcurrentSqlStorage<TEntry, in TKey>
        where TEntry : class, ISqlEntity<TKey>
    {
        [CanBeNull]
        TEntry TryRead(TKey id);

        [NotNull, ItemNotNull]
        TEntry[] TryRead([NotNull] TKey[] ids);

        [NotNull, ItemNotNull]
        TEntry[] ReadAll();

        void CreateOrUpdate([NotNull] TEntry entity, [CanBeNull] Expression<Func<TEntry, object>> onExpression = null, [CanBeNull] Expression<Func<TEntry, TEntry, TEntry>> whenMatched = null);
        void CreateOrUpdate([NotNull, ItemNotNull] TEntry[] entities, [CanBeNull] Expression<Func<TEntry, object>> onExpression = null, [CanBeNull] Expression<Func<TEntry, TEntry, TEntry>> whenMatched = null);

        void Delete([NotNull] TKey[] ids);
        void Delete(TKey id);
        void Delete([NotNull] Expression<Func<TEntry, bool>> criterion);

        [NotNull, ItemNotNull]
        TEntry[] Find([NotNull] Expression<Func<TEntry, bool>> criterion, int limit);

        [NotNull, ItemNotNull]
        TEntry[] Find<TOrderProp>([NotNull] Expression<Func<TEntry, bool>> criterion, Expression<Func<TEntry, TOrderProp>> orderBy, int limit);

        void Batch([NotNull] Action<ISqlStorage> batchAction, IsolationLevel isolationLevel);
    }
}