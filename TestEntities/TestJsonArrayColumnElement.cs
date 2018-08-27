using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;
using SKBKontur.Catalogue.EDI.SqlStorageCore.Mapping;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities
{
    public class TestJsonArrayColumnElement : IdentifiableSqlEntity
    {
        [JsonColumn]
        public TestComplexColumnElement[] ComplexArrayColumn { get; set; }
    }
}