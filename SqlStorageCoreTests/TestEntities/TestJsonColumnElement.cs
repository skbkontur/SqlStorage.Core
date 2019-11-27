using SkbKontur.SqlStorageCore.Schema;

namespace SkbKontur.SqlStorageCore.Tests.TestEntities
{
    public class TestJsonColumnElement : SqlEntity
    {
        [JsonColumn]
        public TestComplexColumnElement ComplexColumn { get; set; }
    }
}