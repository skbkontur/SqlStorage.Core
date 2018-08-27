using System;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Entities
{
    public interface IIdentifiableSqlEntity
    {
        Guid Id { get; set; }
    }
}