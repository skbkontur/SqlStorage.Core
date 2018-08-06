using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.DatabaseContext
{
    public abstract class EntitiesRegistry
    {
        protected void RegisterEntityType<T>()
            where T : IIdentifiableEntity
        {
            entitiesTypes.Add(typeof(T));
        }

        [NotNull]
        public IEnumerable<Type> GetEntitesTypes()
        {
            return entitiesTypes;
        }

        private readonly List<Type> entitiesTypes = new List<Type>();
    }
}