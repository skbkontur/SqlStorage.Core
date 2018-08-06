﻿using JetBrains.Annotations;

using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Storage
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public interface IEntityStorageFactory
    {
        IEntityStorage<T> Create<T>() where T : class, IIdentifiableEntity;
    }
}