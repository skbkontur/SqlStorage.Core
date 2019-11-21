using System;

using SKBKontur.Catalogue.EDI.SqlStorageCore;
using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests
{
    public class TestSqlEntitiesRegistry : SqlEntitiesRegistry
    {
        public TestSqlEntitiesRegistry()
        {
            RegisterEntityType<TestValueTypedPropertiesStorageElement, Guid>();
            RegisterEntityType<TestTimestampElement, Guid>();
            RegisterEntityType<TestJsonColumnElement, Guid>();
            RegisterEntityType<TestJsonArrayColumnElement, Guid>();
            RegisterEntityType<TestUpsertSqlEntry, Guid>();
            RegisterEntityType<TestBatchStorageElement, Guid>();
        }
    }
}