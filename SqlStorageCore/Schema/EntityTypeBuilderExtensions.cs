using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Newtonsoft.Json;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.SqlStorageCore.Schema
{
    public static class EntityTypeBuilderExtensions
    {
        private static readonly ValueConverter<Timestamp, long> timestampConverter = new ValueConverter<Timestamp, long>(timestamp => timestamp.Ticks, l => new Timestamp(l));

        [NotNull]
        public static EntityTypeBuilder ApplyTimestampConverter([NotNull] this EntityTypeBuilder entityTypeBuilder)
        {
            var timestampProperties = entityTypeBuilder
                                      .Metadata
                                      .ClrType
                                      .GetProperties()
                                      .Where(pi => pi.CanRead && pi.CanWrite && pi.PropertyType == typeof(Timestamp));

            foreach (var timestampProperty in timestampProperties)
            {
                entityTypeBuilder
                    .Property(timestampProperty.PropertyType, timestampProperty.Name)
                    .HasConversion(timestampConverter);
            }

            return entityTypeBuilder;
        }

        [NotNull]
        public static EntityTypeBuilder ApplyJsonColumns([NotNull] this EntityTypeBuilder entityTypeBuilder)
        {
            var jsonColumnProperties = ExtractPropertiesMappedWithAttribute<JsonColumnAttribute>(entityTypeBuilder);
            foreach (var (propertyInfo, _) in jsonColumnProperties)
            {
                var converter = new RuntimeValueConverter(
                    propertyInfo.PropertyType,
                    typeof(string),
                    o => JsonConvert.SerializeObject(o),
                    o => JsonConvert.DeserializeObject(o as string, propertyInfo.PropertyType));
                entityTypeBuilder
                    .Property(propertyInfo.PropertyType, propertyInfo.Name)
                    .HasColumnType("json")
                    .HasConversion(converter);
            }

            return entityTypeBuilder;
        }

        [NotNull]
        private static IEnumerable<(PropertyInfo Property, TAttribute[] Attributes)> ExtractPropertiesMappedWithAttribute<TAttribute>([NotNull] EntityTypeBuilder entityTypeBuilder)
            where TAttribute : Attribute
        {
            var jsonColumnProperties = entityTypeBuilder
                                       .Metadata
                                       .ClrType
                                       .GetProperties()
                                       .Where(pi => pi.CanRead && pi.CanWrite)
                                       .Select(pi => (pi, pi.GetCustomAttributes<TAttribute>(inherit : false).ToArray()))
                                       .Where(t => t.Item2.Any());
            return jsonColumnProperties;
        }

        [NotNull]
        public static EntityTypeBuilder HasEventLogWriteTrigger([NotNull] this EntityTypeBuilder entityTypeBuilder)
        {
            entityTypeBuilder.Metadata.SetAnnotation(SqlAnnotations.EventLogTrigger, true);
            return entityTypeBuilder;
        }

        [NotNull]
        public static EntityTypeBuilder ApplyIndices([NotNull] this EntityTypeBuilder entityTypeBuilder)
        {
            var indexedProperties = ExtractPropertiesMappedWithAttribute<IndexedColumnAttribute>(entityTypeBuilder);
            foreach (var (property, attributes) in indexedProperties)
            {
                var indexAttribute = attributes.Single();
                var indexBuilder = entityTypeBuilder.HasIndex(property.Name);
                if (indexAttribute.IndexType != IndexType.BTree)
                    indexBuilder.ForNpgsqlHasMethod(ToNpgsqlIndexName(indexAttribute.IndexType));
            }
            return entityTypeBuilder;
        }

        [NotNull]
        private static string ToNpgsqlIndexName(IndexType indexType)
        {
            switch (indexType)
            {
            case IndexType.BTree:
                return "b-tree";
            case IndexType.Hash:
                return "hash";
            case IndexType.Brin:
                return "brin";
            default:
                throw new ArgumentOutOfRangeException(nameof(indexType), indexType, "Unsupported index type");
            }
        }

        [NotNull]
        public static EntityTypeBuilder ApplyUniqueConstraints([NotNull] this EntityTypeBuilder entityTypeBuilder)
        {
            var uniqueProperties = ExtractPropertiesMappedWithAttribute<UniqueConstraintAttribute>(entityTypeBuilder);
            var uniqueGroups = uniqueProperties.SelectMany(t => t.Attributes.Select(a => (GroupName : a.GroupName ?? t.Property.Name, a.Order, PropertyName : t.Property.Name)))
                                               .OrderBy(t => t.Order)
                                               .GroupBy(t => t.GroupName);
            foreach (var uniqueGroup in uniqueGroups)
            {
                entityTypeBuilder.HasIndex(uniqueGroup.Select(g => g.PropertyName).ToArray()).IsUnique(unique : true);
            }

            return entityTypeBuilder;
        }
    }
}