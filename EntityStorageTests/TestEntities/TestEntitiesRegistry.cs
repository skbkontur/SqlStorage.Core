using SKBKontur.Catalogue.EDI.SqlStorageCore.DatabaseContext;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EntityStorageTests.TestEntities
{
    public class TestEntitiesRegistry : EntitiesRegistry
    {
        public TestEntitiesRegistry()
        {
            RegisterEntityType<TestValueTypedPropertiesStorageElement>();
            RegisterEntityType<TestTimestampElement>();
            RegisterEntityType<TestJsonColumnElement>();
            RegisterEntityType<TestJsonArrayColumnElement>();
        }
    }
}