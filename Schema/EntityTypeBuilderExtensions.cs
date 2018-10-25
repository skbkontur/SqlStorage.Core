using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Newtonsoft.Json;

using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Schema
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
            foreach (var propertyInfo in jsonColumnProperties)
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
        private static IEnumerable<PropertyInfo> ExtractPropertiesMappedWithAttribute<TAttribute>([NotNull] EntityTypeBuilder entityTypeBuilder)
            where TAttribute : Attribute
        {
            var attributeType = typeof(TAttribute);
            var jsonColumnProperties = entityTypeBuilder
                .Metadata
                .ClrType
                .GetProperties()
                .Where(pi => pi.CanRead && pi.CanWrite && pi.GetCustomAttributes(attributeType, inherit : false).Any());
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
            foreach (var property in indexedProperties)
                entityTypeBuilder.HasIndex(property.Name);
            return entityTypeBuilder;
        }

        [NotNull]
        public static EntityTypeBuilder ApplyUniqueConstraints([NotNull] this EntityTypeBuilder entityTypeBuilder)
        {
            var uniqueProperties = ExtractPropertiesMappedWithAttribute<UniqueConstraintAttribute>(entityTypeBuilder);
            var uniqueGroups = uniqueProperties.SelectMany(p => p.GetCustomAttributes(typeof(UniqueConstraintAttribute), inherit : false)
                                                                 .Cast<UniqueConstraintAttribute>()
                                                                 .Select(a => (GroupName: a.GroupName ?? p.Name, a.Order, PropertyName: p.Name)))
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