using System;

using SkbKontur.SqlStorageCore.Benchmarks.Entries;

namespace SkbKontur.SqlStorageCore.Benchmarks
{
    public class BenchmarkSqlEntitiesRegistry : SqlEntitiesRegistry
    {
        public BenchmarkSqlEntitiesRegistry()
        {
            RegisterEntityType<Employee, Guid>();
        }
    }
}