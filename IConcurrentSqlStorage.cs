using System;
using System.Data;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore
{
    public interface IConcurrentSqlStorage<TEntry, in TKey> : ISqlStorage<TEntry, TKey>
        where TEntry : class, ISqlEntity<TKey>
    {
        void Batch([NotNull] Action<ISqlStorage<TEntry, TKey>> batchAction, IsolationLevel isolationLevel);
    }
}