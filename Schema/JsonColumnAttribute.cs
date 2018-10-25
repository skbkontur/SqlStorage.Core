using System;
using System.Diagnostics.CodeAnalysis;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Schema
{
    [SuppressMessage("ReSharper", "RedundantAttributeUsageProperty")]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class JsonColumnAttribute : Attribute
    {
    }
}