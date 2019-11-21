using SKBKontur.Catalogue.EDI.SqlStorageCore;
using SKBKontur.Catalogue.EDI.SqlStorageCore.Schema;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities
{
    public class TestJsonColumnElement : SqlEntity
    {
        [JsonColumn]
        public TestComplexColumnElement ComplexColumn { get; set; }
    }
}