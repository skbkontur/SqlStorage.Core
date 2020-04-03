using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

using AutoFixture;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

using FluentAssertions;

using GroboContainer.NUnitExtensions;

using Microsoft.Extensions.Logging.Abstractions;

using MoreLinq;

using NUnit.Framework;

using SkbKontur.SqlStorageCore.Tests.TestEntities;
using SkbKontur.SqlStorageCore.Tests.TestUtils;
using SkbKontur.SqlStorageCore.Tests.TestWrappers;

using Vostok.Metrics;
using Vostok.Metrics.Models;
using Vostok.Metrics.Senders;

namespace SkbKontur.SqlStorageCore.Tests.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net472, baseline: true)]
    [SimpleJob(RuntimeMoniker.NetCoreApp30)]
    [SimpleJob(RuntimeMoniker.CoreRt30)]
    [SimpleJob(RuntimeMoniker.Mono)]
    [RPlotExporter]
    public class ReadOperationsBenchmark
    {
        // private ConcurrentSqlStorage<TestValueTypedPropertiesStorageElement, Guid> storage;
        // private TestValueTypedPropertiesStorageElement entity;

        private IEnumerable<TestValueTypedPropertiesStorageElement> entities;
        private IConcurrentSqlStorage<TestValueTypedPropertiesStorageElement, Guid> sqlStorage;

        [Params(1000, 10000)]
        public int N;

        [GlobalSetup]
        public async Task Setup()
        {
            sqlStorage = new ConcurrentSqlStorage<TestValueTypedPropertiesStorageElement, Guid>(()=> 
                                                                                                    new SqlDbContext(new TestSqlDbContextSettings("benchmarks", new TestSqlEntitiesRegistry(), this.GetType().Assembly), new NullLoggerFactory() ));
            entities = new Fixture().Build<TestValueTypedPropertiesStorageElement>().CreateMany(N); 
            await sqlStorage.CreateOrUpdateAsync(entities.ToArray());
        }
        

        [Benchmark]
        public async Task ReadSingle()
        {
            entities.ForEach(async entity => await sqlStorage.TryReadAsync(entity.Id));
            //entities.ForEach( entity =>  sqlStorage.TryReadAsync(entity.Id));
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

    }

}