﻿using System.Reflection;

using JetBrains.Annotations;

using SKBKontur.Catalogue.EDI.SqlStorageCore.DatabaseContext;
using SKBKontur.Catalogue.EDIFunctionalTests.Commons.TestWrappers;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery.Impl.TestContext;
using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestWrappers
{
    [WithPostgres(databaseName : "Tests")]
    public class WithTestSqlStorage : EdiTestSuiteWrapperAttribute
    {
        public override sealed void SetUp([NotNull] string suiteName, [NotNull] Assembly testAssembly, [NotNull] IEditableEdiTestContext suiteContext)
        {
            suiteContext.Container.Configurator.ForAbstraction<SqlEntitiesRegistry>().UseInstances(new TestSqlEntitiesRegistry());
            using (var dbContext = suiteContext.Container.Create<SqlDatabaseContext>())
                dbContext.Database.EnsureCreated();
        }
    }
}