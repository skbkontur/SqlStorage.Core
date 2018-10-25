using System;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Schema
{
    [SuppressMessage("ReSharper", "RedundantAttributeUsageProperty")]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class UniqueConstraintAttribute : Attribute
    {
        public UniqueConstraintAttribute([CanBeNull] string groupName, int order)
        {
            GroupName = groupName;
            Order = order;
        }

        [CanBeNull]
        public string GroupName { get; }

        public int Order { get; }
    }
}