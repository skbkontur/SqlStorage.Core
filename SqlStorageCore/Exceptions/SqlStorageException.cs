using System;

namespace SkbKontur.SqlStorageCore.Exceptions
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