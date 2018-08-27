using System;
using System.Linq.Expressions;

using JetBrains.Annotations;

using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Storage
{
    public interface ISqlStorage<T> where T : class, IIdentifiableSqlEntity
    {
        [CanBeNull]
        T TryRead(Guid id);

        [NotNull, ItemNotNull]
        T[] TryRead([NotNull] Guid[] ids);

        [NotNull, ItemNotNull]
        T[] ReadAll();

        void CreateOrUpdate([NotNull] T entity);
        void CreateOrUpdate([NotNull, ItemNotNull] T[] entities);

        void Create([NotNull] T entity);
        void Create([NotNull, ItemNotNull] T[] entities);

        void Update([NotNull] T entity);
        void Update([NotNull, ItemNotNull] T[] entities);

        void Delete([NotNull] Guid[] ids);
        void Delete(Guid id);
        void Delete([NotNull] Expression<Func<T, bool>> criterion);

        [NotNull, ItemNotNull]
        T[] Find([NotNull] Expression<Func<T, bool>> criterion, int start, int limit);

        int GetCount([NotNull] Expression<Func<T, bool>> criterion);

        [CanBeNull]
        TValue GetMaxValue<TValue>([NotNull] Expression<Func<T, TValue>> propertySelector);

        [CanBeNull]
        TValue GetMaxValue<TValue>([NotNull] Expression<Func<T, TValue>> propertySelector, [NotNull] Expression<Func<T, bool>> filter);
    }
}