using System;

using SkbKontur.SqlStorageCore.Tests.TestEntities;

namespace SkbKontur.SqlStorageCore.Tests
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
            RegisterEntityType<TestCustomJsonConverterSqlEntity, Guid>();
        }
    }
}