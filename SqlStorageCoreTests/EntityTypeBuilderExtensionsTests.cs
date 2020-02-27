using System;
using System.Net;
using System.Threading.Tasks;

using FluentAssertions;

using GroboContainer.NUnitExtensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
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
    [AndSqlStorageCleanUp(typeof(TestCustomJsonConverterSqlEntity))] //?
    public class EntityTypeBuilderExtensionsTests
    {
        [Test]
        public async Task TestApplyJsonColumn()
        {
            ISqlDbContextSettings settings = new TestSqlDbContextSettings(sqlDbContextSettings.Database, sqlDbContextSettings.SqlEntitiesRegistry, sqlDbContextSettings.MigrationsAssembly)
            {
                CustomJsonConverters = new[] { (JsonConverter)new TestCustomJsonConverterSqlEntryJsonConverter() }
            };
            SqlDbContext CreateContext() => new SqlDbContext(settings, loggerFactory);
            var storage = new ConcurrentSqlStorage<TestCustomJsonConverterSqlEntity, Guid>(CreateContext);
            var entity = new TestCustomJsonConverterSqlEntity { CustomJsonColumn = new TestCustomJsonConverterColumnElement(123, "Some property") };

            //var e = new SqlDbContext(settings, loggerFactory).GetService<IModel>().FindEntityType(entity.GetType());
            storage.CreateOrUpdate(entity);

            var relational = new SqlDbContext(settings, loggerFactory).Model.FindEntityType(entity.GetType()).Relational();
            var connectionString = new NpgsqlConnectionStringBuilder
                {
                    Host = settings.Host,
                    Username = settings.Username,
                    Password = settings.Password,
                    Database = settings.Database,
                };
            using var conn = new NpgsqlConnection(connectionString.ToString());
            
            await conn.OpenAsync();

            string resultEntity;
            using (var cmd = new NpgsqlCommand($@"SELECT CustomJsonColumn FROM {relational.TableName} WHERE Id == {entity.Id}", conn))
            {
                using var reader = await cmd.ExecuteReaderAsync();
                resultEntity = reader.GetString(0);
            }
            resultEntity.Should().BeEquivalentTo($"123{TestCustomJsonConverterSqlEntryJsonConverter.FieldsDelimeter}Some property");
        }

        //[Injected]
        //private readonly IConcurrentSqlStorage<TestCustomJsonConverterSqlEntity, Guid> storage;

        [Injected]
        private readonly ISqlDbContextSettings sqlDbContextSettings;

        [Injected]
        private readonly ILoggerFactory loggerFactory;
    }
}