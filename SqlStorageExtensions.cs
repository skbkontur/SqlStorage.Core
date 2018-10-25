using System;
using System.Linq;
using System.Linq.Expressions;

using AgileObjects.ReadableExpressions;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Expressions;
using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore
{
    public static class SqlStorageExtensions
    {
        [CanBeNull]
        public static T FindSingleOrDefault<T, TKey>([NotNull] this ISqlStorage<T, TKey> storage, [NotNull] Expression<Func<T, bool>> criterion)
            where T : class, ISqlEntity<TKey>
        {
            var searchResult = storage.Find(criterion);
            if (searchResult.Length > 1)
            {
                var criterionReadable = criterion.ToSimplifiedExpression().Expression.ToReadableString();
                throw new InvalidProgramStateException($"Found more than one {typeof(T).Name} trying to get single. Criterion: {criterionReadable}");
            }

            return searchResult.FirstOrDefault();
        }
    }
}