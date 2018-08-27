using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.EventLog
{
    public interface ISqlEventLogRepository<TEntity> where TEntity : IIdentifiableSqlEntity
    {
        [NotNull, ItemNotNull]
        SqlEvent<TEntity>[] GetEvents(long fromOffsetExclusive, long toOffsetInclusive, int limit);

        int GetCount(long fromOffsetExclusive, long toOffsetInclusive);

        long GetLastOffset();

        long GetLastOffsetForTimestamp(DateTime timestamp);

        DateTime GetMaxTimestampForOffset(long offset);
    }
}