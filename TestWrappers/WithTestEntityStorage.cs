using System.Reflection;

using JetBrains.Annotations;

using SKBKontur.Catalogue.EDI.SqlStorageCore.DatabaseContext;
using SKBKontur.Catalogue.EDIFunctionalTests.Commons.TestWrappers;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery.Impl.TestContext;
using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EntityStorageTests.TestEntities;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestWrappers
{
    [WithPostgres(databaseName : "Tests")]
    public class WithTestEntityStorage : EdiTestSuiteWrapperAttribute
    {
        public override sealed void SetUp([NotNull] string suiteName, [NotNull] Assembly testAssembly, [NotNull] IEditableEdiTestContext suiteContext)
        {
            suiteContext.Container.Configurator.ForAbstraction<EntitiesRegistry>().UseInstances(new TestEntitiesRegistry());
            using (var dbContext = suiteContext.Container.Create<EntitiesDatabaseContext>())
                dbContext.Database.EnsureCreated();
        }
    }
}