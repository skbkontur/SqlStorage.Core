using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;
using SKBKontur.Catalogue.Objects;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EntityStorageTests.TestEntities
{
    public class TestTimestampElement : IdentifiableEntity
    {
        public Timestamp Timestamp { get; set; }
    }
}