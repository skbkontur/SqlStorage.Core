using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Entities
{
    public class IdentifiableSqlEntity : IIdentifiableSqlEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
    }
}