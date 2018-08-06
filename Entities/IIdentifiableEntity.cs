using System;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Entities
{
    public interface IIdentifiableEntity
    {
        Guid Id { get; set; }
    }
}