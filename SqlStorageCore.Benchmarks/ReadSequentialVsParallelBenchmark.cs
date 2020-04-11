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
    public class ReadSequentialVsParallel
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
        public async Task ReadSequential()
        {
            foreach (var entity in entities)
            {
                await sqlStorage.TryReadAsync(entity.Id);
            }
        }

        [Benchmark]
        public async Task ReadParallel()
        {
            Task.WaitAll(entities.Select(entity=>sqlStorage.TryReadAsync(entity.Id)).ToArray());
        }

        private IEnumerable<Employee> entities;
        private IConcurrentSqlStorage<Employee, Guid> sqlStorage;

        [Params(1000, 10000)]
        // ReSharper disable once UnassignedField.Global
        public int entriesCount;
    }
}