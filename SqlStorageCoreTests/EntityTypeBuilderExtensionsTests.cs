using System;
using System.Threading.Tasks;

using FluentAssertions;

using GroboContainer.NUnitExtensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Npgsql;

using NUnit.Framework;

using SkbKontur.SqlStorageCore.Tests.TestEntities;
using SkbKontur.SqlStorageCore.Tests.TestUtils;
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
            ISqlDbContextSettings settings = new TestSqlDbContextSettings(sqlDbContextSettings.Database, sqlDbContextSettings.SqlEntitiesRegistry, sqlDbContextSettings.MigrationsAssembly)
                {
                    CustomJsonConverters = new[] {(JsonConverter)new TestCustomJsonConverterSqlEntryJsonConverter()}
                };
            SqlDbContext CreateContext() => new SqlDbContext(settings, loggerFactory);
            var storage = new ConcurrentSqlStorage<TestCustomJsonConverterSqlEntity, Guid>(CreateContext);
            var entity = new TestCustomJsonConverterSqlEntity {CustomJsonColumn = new TestCustomJsonConverterColumnElement(123, "Some property")};

            await storage.CreateOrUpdateAsync(entity);

            var connectionString = new NpgsqlConnectionStringBuilder
                {
                    Host = settings.Host,
                    Username = settings.Username,
                    Password = settings.Password,
                    Database = settings.Database,
                };

            await using var conn = new NpgsqlConnection(connectionString.ToString());
            await conn.OpenAsync();

            var tableName = new SqlDbContext(settings, loggerFactory).Model.FindEntityType(entity.GetType()).GetTableName();

            using var cmd = new NpgsqlCommand($@"SELECT ""CustomJsonColumn"" FROM ""{tableName}"" WHERE ""Id"" = '{entity.Id}'", conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            var resultEntity = reader.GetString(0);
            resultEntity.Should().BeEquivalentTo($"123{TestCustomJsonConverterSqlEntryJsonConverter.FieldsDelimiter}Some property");
        }

        [Injected]
        private readonly ISqlDbContextSettings sqlDbContextSettings;

        [Injected]
        private readonly ILoggerFactory loggerFactory;
    }
}