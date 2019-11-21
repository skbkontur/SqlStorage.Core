using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore
{
    public class SqlEntity : ISqlEntity<Guid>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
    }
}