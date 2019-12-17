using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Newtonsoft.Json;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.SqlStorageCore.Schema
{
    public static class EntityTypeBuilderExtensions
    {
        public static EntityTypeBuilder ApplyTimestampConverter(this EntityTypeBuilder entityTypeBuilder)
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

        public static EntityTypeBuilder ApplyJsonColumns(this EntityTypeBuilder entityTypeBuilder, JsonConverter[]? jsonConverters = null)
        {
            var jsonSerializerSettings = new JsonSerializerSettings {Converters = jsonConverters};
            var jsonColumnProperties = ExtractPropertiesMappedWithAttribute<JsonColumnAttribute>(entityTypeBuilder);
            foreach (var (propertyInfo, _) in jsonColumnProperties)
            {
                var converter = new RuntimeValueConverter(
                    propertyInfo.PropertyType,
                    typeof(string),
                    o => JsonConvert.SerializeObject(o, jsonSerializerSettings),
                    o => JsonConvert.DeserializeObject(o as string, propertyInfo.PropertyType, jsonSerializerSettings));
                entityTypeBuilder
                    .Property(propertyInfo.PropertyType, propertyInfo.Name)
                    .HasColumnType("json")
                    .HasConversion(converter);
            }

            return entityTypeBuilder;
        }

        private static IEnumerable<(PropertyInfo Property, TAttribute[] Attributes)> ExtractPropertiesMappedWithAttribute<TAttribute>(EntityTypeBuilder entityTypeBuilder)
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

        public static EntityTypeBuilder HasEventLogWriteTrigger(this EntityTypeBuilder entityTypeBuilder)
        {
            entityTypeBuilder.Metadata.SetAnnotation(SqlAnnotations.EventLogTrigger, true);
            return entityTypeBuilder;
        }

        public static EntityTypeBuilder ApplyIndices(this EntityTypeBuilder entityTypeBuilder)
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

        private static string ToNpgsqlIndexName(IndexType indexType)
        {
            return indexType switch
                {
                    IndexType.BTree => "b-tree",
                    IndexType.Hash => "hash",
                    IndexType.Brin => "brin",
                    _ => throw new ArgumentOutOfRangeException(nameof(indexType), indexType, "Unsupported index type")
                };
        }

        public static EntityTypeBuilder ApplyUniqueConstraints(this EntityTypeBuilder entityTypeBuilder)
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

        private static readonly ValueConverter<Timestamp, long> timestampConverter = new ValueConverter<Timestamp, long>(timestamp => timestamp.Ticks, l => new Timestamp(l));
    }
}