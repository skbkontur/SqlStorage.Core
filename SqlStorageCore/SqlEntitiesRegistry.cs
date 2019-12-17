using System;
using System.Collections.Generic;

namespace SkbKontur.SqlStorageCore
{
    public abstract class SqlEntitiesRegistry
    {
        protected void RegisterEntityType<T, TKey>()
            where T : ISqlEntity<TKey>
        {
            sqlEntityTypes.Add(typeof(T));
        }

        public IEnumerable<Type> GetEntityTypes()
        {
            return sqlEntityTypes;
        }

        private readonly List<Type> sqlEntityTypes = new List<Type>();
    }
}