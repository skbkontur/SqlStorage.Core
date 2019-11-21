using SKBKontur.Catalogue.EDI.SqlStorageCore;
using SKBKontur.Catalogue.EDI.SqlStorageCore.Schema;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities
{
    public class TestJsonArrayColumnElement : SqlEntity
    {
        [JsonColumn]
        public TestComplexColumnElement[] ComplexArrayColumn { get; set; }
    }
}