using SKBKontur.Catalogue.EDI.SqlStorageCore;
using SKBKontur.Catalogue.Objects;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities
{
    public class TestTimestampElement : SqlEntity
    {
        public Timestamp Timestamp { get; set; }
    }
}