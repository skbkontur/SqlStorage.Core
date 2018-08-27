using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;
using SKBKontur.Catalogue.EDI.SqlStorageCore.Mapping;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities
{
    public class TestJsonColumnElement : IdentifiableSqlEntity
    {
        [JsonColumn]
        public TestComplexColumnElement ComplexColumn { get; set; }
    }
}