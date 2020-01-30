using System;

using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using Microsoft.EntityFrameworkCore;

namespace SkbKontur.SqlStorageCore.Tests.TestWrappers
{
    public class AndSqlStorageCleanUp : GroboTestMethodWrapperAttribute
    {
        public AndSqlStorageCleanUp(Type entityType)
        {
            this.entityType = entityType;
        }

        protected override string? TryGetIdentity()
        {
            return entityType.FullName;
        }

        public override sealed void SetUp(string testName, IEditableGroboTestContext suiteContext, IEditableGroboTestContext methodContext)
        {
            using var context = suiteContext.Container.Create<SqlDbContext>();
            var tableName = context.Model.FindEntityType(entityType).GetTableName();
            var sql = $@"TRUNCATE ""{tableName}"";";
#pragma warning disable EF1000 // Possible SQL injection vulnerability.
            context.Database.ExecuteSqlCommand(sql);
#pragma warning disable EF1000 // Possible SQL injection vulnerability.
        }

        private readonly Type entityType;
    }
}