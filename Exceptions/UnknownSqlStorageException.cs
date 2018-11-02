using System;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Exceptions
{
    public class UnknownSqlStorageException : SqlStorageException
    {
        public UnknownSqlStorageException(Exception innerException)
            : base("Unknown exception in SqlStorage", innerException)
        {
        }
    }
}