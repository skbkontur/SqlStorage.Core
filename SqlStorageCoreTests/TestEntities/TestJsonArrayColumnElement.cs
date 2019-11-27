using SkbKontur.SqlStorageCore.Schema;

namespace SkbKontur.SqlStorageCore.Tests.TestEntities
{
    public class TestJsonArrayColumnElement : SqlEntity
    {
        [JsonColumn]
        public TestComplexColumnElement[] ComplexArrayColumn { get; set; }
    }
}