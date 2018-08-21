using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using MoreLinq;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.DataGenertation
{
    public static class DataGenerator
    {
        public static IEnumerable<T> Random<T>() where T : new()
        {
            var random = new Random(DateTime.Now.Millisecond);
            var generator = new RandomObjectGenerator<T>(random);
            return generator.Generate();
        }

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