using System;

using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using JetBrains.Annotations;

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
            var relational = context.Model.FindEntityType(entityType).Relational();
            var sql = $@"TRUNCATE ""{relational.TableName}"";";
#pragma warning disable EF1000 // Possible SQL injection vulnerability.
            context.Database.ExecuteSqlCommand(sql);
#pragma warning disable EF1000 // Possible SQL injection vulnerability.
        }

        private readonly Type entityType;
    }
}