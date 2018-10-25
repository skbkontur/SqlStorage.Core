using JetBrains.Annotations;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.EventLog
{
    public interface ISqlEventLogRepository<TEntity, TKey> where TEntity : ISqlEntity<TKey>
    {
        [NotNull, ItemNotNull]
        SqlEvent<TEntity>[] GetEvents(long? fromOffsetExclusive, int limit);
    }
}