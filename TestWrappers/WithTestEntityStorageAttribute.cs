using System.Reflection;

using SKBKontur.Catalogue.EDI.SqlStorageCore.DatabaseContext;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery.Impl.TestContext;
using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EntityStorageTests.TestEntities;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestWrappers
{
    [WithTestPostgres]
    public class WithTestEntityStorageAttribute : EdiTestSuiteWrapperAttribute
    {
        public override void SetUp(string suiteName, Assembly testAssembly, IEditableEdiTestContext suiteContext)
        {
            suiteContext.Container.Configurator.ForAbstraction<EntitiesRegistry>().UseInstances(new TestEntitiesRegistry());
            using (var dbContext = suiteContext.Container.Create<EntitiesDatabaseContext>())
            {
                dbContext.Database.EnsureCreated();
            }
        }
    }
}