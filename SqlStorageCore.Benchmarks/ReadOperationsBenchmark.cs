using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoFixture;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

using SkbKontur.SqlStorageCore.Benchmarks.Entries;
using SkbKontur.SqlStorageCore.Benchmarks.Migrations;
using SkbKontur.SqlStorageCore.Schema;

using Vostok.Logging.Abstractions;

namespace SkbKontur.SqlStorageCore.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net472, baseline : true)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [RPlotExporter]
    public class ReadOperationsBenchmark
    {
        [GlobalSetup]
        public async Task Setup()
        {
            var sqlStorageMigrator = new SqlStorageMigrator(SqlDbContextFactory.CreateDbContext, new SilentLog());
            await sqlStorageMigrator.MigrateAsync();
            sqlStorage = new ConcurrentSqlStorage<Employee, Guid>(SqlDbContextFactory.CreateDbContext);
            entities = new Fixture().Build<Employee>().CreateMany(entriesCount);
            await sqlStorage.CreateOrUpdateAsync(entities.ToArray());
        }

        [Benchmark]
        public async Task ReadSingle()
        {
            foreach (var entity in entities)
            {
                await sqlStorage.TryReadAsync(entity.Id);
            }
        }

        [Benchmark]
        public async Task ReadArray()
        {
            await sqlStorage.TryReadAsync(entities.Select(e => e.Id).ToArray());
        }

        [Benchmark]
        public async Task ReadAll()
        {
            await sqlStorage.ReadAllAsync();
        }

        // private ConcurrentSqlStorage<TestValueTypedPropertiesStorageElement, Guid> storage;
        // private TestValueTypedPropertiesStorageElement entity;

        private IEnumerable<Employee> entities;
        private IConcurrentSqlStorage<Employee, Guid> sqlStorage;

        [Params(1000, 10000)]
        // ReSharper disable once UnassignedField.Global
        public int entriesCount;
    }
}