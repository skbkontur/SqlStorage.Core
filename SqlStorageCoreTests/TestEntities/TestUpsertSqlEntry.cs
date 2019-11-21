using System;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

using SKBKontur.Catalogue.EDI.SqlStorageCore;
using SKBKontur.Catalogue.EDI.SqlStorageCore.Schema;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities
{
    public class TestUpsertSqlEntry : SqlEntity
    {
        [UniqueConstraint(groupName : "Ids", order : 0)]
        public Guid SomeId1 { get; set; }

        [UniqueConstraint(groupName : "Ids", order : 1)]
        public Guid SomeId2 { get; set; }

        [CanBeNull]
        public string StringValue { get; set; }

        [Required]
        public string RequiredValue { get; set; }
    }
}