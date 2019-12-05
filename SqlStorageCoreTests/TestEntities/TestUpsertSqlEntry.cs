using System;
using System.ComponentModel.DataAnnotations;

using JetBrains.Annotations;

using SkbKontur.SqlStorageCore.Schema;

namespace SkbKontur.SqlStorageCore.Tests.TestEntities
{
    public class TestUpsertSqlEntry : SqlEntity
    {
        [UniqueConstraint(groupName : "Ids", order : 0)]
        public Guid SomeId1 { get; set; }

        [UniqueConstraint(groupName : "Ids", order : 1)]
        public Guid SomeId2 { get; set; }

        public string? StringValue { get; set; }

        [Required]
        public string? RequiredValue { get; set; }
    }
}