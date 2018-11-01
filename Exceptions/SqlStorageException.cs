using System;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Exceptions
{
    public abstract class SqlStorageException : Exception
    {
        protected SqlStorageException(string message, string sqlStatus)
            : base(message)
        {
            SqlStatus = sqlStatus;
        }

        protected SqlStorageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        [CanBeNull]
        public string SqlStatus { get; }
    }
}