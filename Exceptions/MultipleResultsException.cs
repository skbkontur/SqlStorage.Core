using System;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Exceptions
{
    public class MultipleResultsException : Exception
    {
        public MultipleResultsException(string message)
            : base(message)
        {
        }
    }
}