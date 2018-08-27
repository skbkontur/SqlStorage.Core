using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.EDI.SqlStorageCore.EventLog;
using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.EventFeeds
{
    public class SqlStorageTimeProvider : IGlobalTimeProvider
    {
        public SqlStorageTimeProvider(SqlStorageTimeRepository sqlStorageTimeRepository)
        {
            this.sqlStorageTimeRepository = sqlStorageTimeRepository;
        }

        [NotNull]
        public Timestamp GetNowTimestamp()
        {
            var storageDateTime = sqlStorageTimeRepository.GetCurrentStorageTime();
            return new Timestamp(storageDateTime.ToUniversalTime());
        }

        private readonly SqlStorageTimeRepository sqlStorageTimeRepository;
    }
}