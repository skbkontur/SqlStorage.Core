using System.Threading;
using System.Threading.Tasks;

namespace SkbKontur.SqlStorageCore.EventLog
{
    public interface ISqlEventLogRepository<TEntity, TKey>
        where TEntity : ISqlEntity<TKey>
        where TKey : notnull
    {
        Task<SqlEvent<TEntity>[]> GetEventsAsync(long? fromOffsetExclusive, int limit, CancellationToken cancellationToken = default);

        Task<int> GetEventsCountAsync(long? fromOffsetExclusive, CancellationToken cancellationToken = default);
    }
}