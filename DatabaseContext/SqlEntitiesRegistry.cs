using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.DatabaseContext
{
    public abstract class SqlEntitiesRegistry
    {
        protected void RegisterEntityType<T>()
            where T : IIdentifiableSqlEntity
        {
            sqlEntityTypes.Add(typeof(T));
        }

        [NotNull]
        public IEnumerable<Type> GetEntitesTypes()
        {
            return sqlEntityTypes;
        }

        private readonly List<Type> sqlEntityTypes = new List<Type>();
    }
}