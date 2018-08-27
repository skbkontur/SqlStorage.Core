using SKBKontur.Catalogue.EDI.SqlStorageCore.DatabaseContext;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities
{
    public class TestSqlEntitiesRegistry : SqlEntitiesRegistry
    {
        public TestSqlEntitiesRegistry()
        {
            RegisterEntityType<TestValueTypedPropertiesStorageElement>();
            RegisterEntityType<TestTimestampElement>();
            RegisterEntityType<TestJsonColumnElement>();
            RegisterEntityType<TestJsonArrayColumnElement>();
        }
    }
}