using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;
using SKBKontur.Catalogue.EDI.SqlStorageCore.Mapping;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EntityStorageTests.TestEntities
{
    public class TestJsonColumnElement : IdentifiableEntity
    {
        [JsonColumn]
        public TestComplexColumnElement ComplexColumn { get; set; }
    }

    public class TestJsonArrayColumnElement : IdentifiableEntity
    {
        [JsonColumn]
        public TestComplexColumnElement[] ComplexArrayColumn { get; set; }
    }

    public class TestComplexColumnElement
    {
        public string StringProperty { get; set; }
        public int IntProperty { get; set; }
    }
}