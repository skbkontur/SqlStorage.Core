using System;
using System.Diagnostics.CodeAnalysis;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Schema
{
    [SuppressMessage("ReSharper", "RedundantAttributeUsageProperty")]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class IndexedColumnAttribute : Attribute
    {
        public IndexedColumnAttribute(IndexType indexType = IndexType.BTree)
        {
            IndexType = indexType;
        }

        public IndexType IndexType { get; }
    }
}