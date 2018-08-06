using SKBKontur.Catalogue.EDIFunctionalTests.Commons.TestWrappers;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestWrappers
{
    public class WithTestPostgresAttribute : WithPostgresAttribute
    {
        public WithTestPostgresAttribute()
            : base(testsDatabaseName)
        {
        }

        private const string testsDatabaseName = "Tests";
    }
}