using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging.Abstractions;

namespace SkbKontur.SqlStorageCore.Benchmarks.Migrations
{
    /// <inheritdoc />
    /// <summary>
    ///     Factory used by EntityFrameworkCore.Tools to create migrations
    /// </summary>
    public class SqlDbContextFactory : IDesignTimeDbContextFactory<SqlDbContext>
    {
        public SqlDbContext CreateDbContext(string[] args) => CreateDbContext();

        public static SqlDbContext CreateDbContext() => new SqlDbContext(new BenchmarksSqlDbContextSettings(), new NullLoggerFactory());
    }
}