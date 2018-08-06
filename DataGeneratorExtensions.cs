using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using MoreLinq;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests
{
    public static class DataGeneratorExtensions
    {
        public static IEnumerable<T> WithGuidProperty<T>(this IEnumerable<T> list, Expression<Func<T, Guid>> propertyAccessor)
        {
            return list.WithProperty(propertyAccessor, Guid.NewGuid);
        }

        private static IEnumerable<T> WithProperty<T, TU>(this IEnumerable<T> list, Expression<Func<T, TU>> propertyAccessor, Func<TU> valueGenerator)
        {
            var propertyInfo = (PropertyInfo)((MemberExpression)propertyAccessor.Body).Member;
            return list.Pipe(x => propertyInfo.SetValue(x, valueGenerator(), new object[0]));
        }
    }
}