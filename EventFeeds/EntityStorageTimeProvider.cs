using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.EDI.SqlStorageCore.EventLog;
using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.EventFeeds
{
    public class EntityStorageTimeProvider : IGlobalTimeProvider
    {
        public EntityStorageTimeProvider(EntitesStorageTimeRepository entitesStorageTimeRepository)
        {
            this.entitesStorageTimeRepository = entitesStorageTimeRepository;
        }

        [NotNull]
        public Timestamp GetNowTimestamp()
        {
            var storageDateTime = entitesStorageTimeRepository.GetCurrentStorageTime();
            return new Timestamp(storageDateTime.ToUniversalTime());
        }

        private readonly EntitesStorageTimeRepository entitesStorageTimeRepository;
    }
}