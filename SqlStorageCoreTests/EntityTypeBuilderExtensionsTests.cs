using System;
using System.Threading.Tasks;

using FluentAssertions;

using GroboContainer.NUnitExtensions;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using NUnit.Framework;

using SkbKontur.SqlStorageCore.Tests.TestEntities;
using SkbKontur.SqlStorageCore.Tests.TestWrappers;

namespace SkbKontur.SqlStorageCore.Tests
{
    [GroboTestSuite, WithTestSqlStorage]
    [AndSqlStorageCleanUp(typeof(TestCustomJsonConverterSqlEntity))]
    public class EntityTypeBuilderExtensionsTests
    {
        [Test]
        public async Task TestApplyJsonColumn()
        {
            var entity = new TestCustomJsonConverterSqlEntity {CustomJsonColumn = new TestCustomJsonConverterColumnElement(123, "Some property")};

            await storage.CreateOrUpdateAsync(entity);

            await using var conn = new NpgsqlConnection(BuildConnectionString());
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand($@"SELECT ""CustomJsonColumn"" FROM ""{GetTableName(entity)}"" WHERE ""Id"" = '{entity.Id}'", conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            var resultEntity = reader.GetString(0);
            resultEntity.Should().BeEquivalentTo($@"""123{TestCustomJsonConverterSqlEntryJsonConverter.FieldsDelimiter}Some property""");

            var actualEntity = await storage.TryReadAsync(entity.Id);
            actualEntity.Should().NotBeNull();
            actualEntity!.CustomJsonColumn.IntProperty.Should().Be(123);
            actualEntity!.CustomJsonColumn.StringProperty.Should().Be("Some property");
        }

        private string GetTableName(TestCustomJsonConverterSqlEntity entity)
        {
            return createDbContext().Model.FindEntityType(entity.GetType()).GetTableName();
        }

        private string BuildConnectionString()
        {
            return createDbContext().Database.GetDbConnection().ConnectionString;
        }

        [Injected]
        private readonly IConcurrentSqlStorage<TestCustomJsonConverterSqlEntity, Guid> storage;

        [Injected]
        private readonly Func<SqlDbContext> createDbContext;
    }
}