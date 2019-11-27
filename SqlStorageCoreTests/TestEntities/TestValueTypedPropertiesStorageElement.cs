using System;

namespace SkbKontur.SqlStorageCore.Tests.TestEntities
{
    public class TestValueTypedPropertiesStorageElement : SqlEntity
    {
        public string StringProperty { get; set; }

        public int? IntProperty { get; set; }

        public bool? BoolProperty { get; set; }

        public DateTime? DateTimeProperty { get; set; }
    }
}