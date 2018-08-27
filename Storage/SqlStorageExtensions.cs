using System;
using System.Linq;
using System.Linq.Expressions;

using JetBrains.Annotations;

using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;
using SKBKontur.Catalogue.EDI.SqlStorageCore.Exceptions;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Storage
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public static class SqlStorageExtensions
    {
        [CanBeNull]
        public static T FindSingleOrDefault<T>([NotNull] this ISqlStorage<T> storage, [NotNull] Expression<Func<T, bool>> criterion)
            where T : class, IIdentifiableSqlEntity
        {
            var searchResult = storage.Find(criterion);
            if (searchResult.Length > 1)
                throw new MultipleResultsException($"Found more than one {typeof(T).Name} trying to get single. Criterion: {criterion}");

            return searchResult.FirstOrDefault();
        }
    }
}