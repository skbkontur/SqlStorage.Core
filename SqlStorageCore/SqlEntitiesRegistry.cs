using System;
using System.Collections.Generic;

namespace SkbKontur.SqlStorageCore
{
    public abstract class SqlEntitiesRegistry
    {
        protected void RegisterEntityType<TEntity, TKey>()
            where TEntity : ISqlEntity<TKey>
            where TKey : notnull
        {
            sqlEntityTypes.Add(typeof(TEntity));
        }

        public IEnumerable<Type> GetEntityTypes()
        {
            return sqlEntityTypes;
        }

        private readonly List<Type> sqlEntityTypes = new List<Type>();
    }
}