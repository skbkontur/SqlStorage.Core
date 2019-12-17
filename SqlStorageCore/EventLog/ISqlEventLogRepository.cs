namespace SkbKontur.SqlStorageCore.EventLog
{
    public interface ISqlEventLogRepository<TEntity, TKey>
        where TEntity : ISqlEntity<TKey>
        where TKey : notnull
    {
        SqlEvent<TEntity>[] GetEvents(long? fromOffsetExclusive, int limit);

        int GetEventsCount(long? fromOffsetExclusive);
    }
}