using System;
using System.Linq.Expressions;

using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SkbKontur.SqlStorageCore.Schema
{
    public class RuntimeValueConverter : ValueConverter
    {
        public RuntimeValueConverter(
            [NotNull] Type modelClrType,
            [NotNull] Type providerClrType,
            [NotNull] Expression<Func<object, object>> convertToProviderExpression,
            [NotNull] Expression<Func<object, object>> convertFromProviderExpression)
            : base(convertToProviderExpression, convertFromProviderExpression, mappingHints : null)
        {
            ModelClrType = modelClrType;
            ProviderClrType = providerClrType;
            ConvertToProvider = convertToProviderExpression.Compile();
            ConvertFromProvider = convertFromProviderExpression.Compile();
        }

        public override Func<object, object> ConvertToProvider { get; }
        public override Func<object, object> ConvertFromProvider { get; }
        public override Type ModelClrType { get; }
        public override Type ProviderClrType { get; }
    }
}