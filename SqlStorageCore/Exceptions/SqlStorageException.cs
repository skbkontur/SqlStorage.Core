using System;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Exceptions
{
    public abstract class SqlStorageException : Exception
    {
        protected SqlStorageException(string message)
            : base(message)
        {
        }

        protected SqlStorageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}