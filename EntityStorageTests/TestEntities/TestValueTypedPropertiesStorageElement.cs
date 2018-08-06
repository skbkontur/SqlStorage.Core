using System;

using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.EntityStorageTests.TestEntities
{
    public class TestValueTypedPropertiesStorageElement : IdentifiableEntity
    {
        public string StringProperty { get; set; }

        public int? IntProperty { get; set; }

        public bool? BoolProperty { get; set; }

        public DateTime? DateTimeProperty { get; set; }
    }
}