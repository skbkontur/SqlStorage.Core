using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;
using SKBKontur.Catalogue.Objects;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities
{
    public class TestTimestampElement : IdentifiableSqlEntity
    {
        public Timestamp Timestamp { get; set; }
    }
}