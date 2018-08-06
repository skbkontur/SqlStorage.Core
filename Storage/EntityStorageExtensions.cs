using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using JetBrains.Annotations;

using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;
using SKBKontur.Catalogue.EDI.SqlStorageCore.Exceptions;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Storage
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public static class EntityStorageExtensions
    {
        [CanBeNull]
        public static T FindSingleOrDefault<T>([NotNull] this IEntityStorage<T> storage, [NotNull] Expression<Func<T, bool>> criterion)
            where T : class, IIdentifiableEntity
        {
            var searchResult = storage.Find(criterion, 0, 2);
            if (searchResult.Length > 1)
                throw new MultipleResultsException($"Found more than one {typeof(T).Name} trying to get single. Criterion: {criterion}");

            return searchResult.FirstOrDefault();
        }

        [NotNull, ItemNotNull]
        public static IEnumerable<T> FindAll<T>([NotNull] this IEntityStorage<T> storage, [NotNull] Expression<Func<T, bool>> criterion)
            where T : class, IIdentifiableEntity
        {
            const int bulkSize = 1000;
            var start = 0;
            while (true)
            {
                var items = storage.Find(criterion, start, bulkSize);
                if (items.Length == 0)
                    yield break;
                start = items.Length - 1;
                foreach (var item in items)
                    yield return item;
                if (items.Length < bulkSize / 2)
                    yield break;
            }
        }
    }
}