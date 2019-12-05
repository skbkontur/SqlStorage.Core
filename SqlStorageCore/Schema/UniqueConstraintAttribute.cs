using System;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace SkbKontur.SqlStorageCore.Schema
{
    [SuppressMessage("ReSharper", "RedundantAttributeUsageProperty")]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class UniqueConstraintAttribute : Attribute
    {
        public UniqueConstraintAttribute()
        {
        }

        public UniqueConstraintAttribute(string? groupName, int order)
        {
            GroupName = groupName;
            Order = order;
        }

        public string? GroupName { get; }

        public int Order { get; }
    }
}