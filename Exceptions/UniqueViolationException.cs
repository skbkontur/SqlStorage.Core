using JetBrains.Annotations;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Exceptions
{
    public sealed class UniqueViolationException : SqlStorageException
    {
        public UniqueViolationException(string message, string constraint, string details)
            : base(message)
        {
            Constraint = constraint;
            Details = details;
        }

        [NotNull]
        public string Constraint { get; }

        [NotNull]
        public string Details { get;}
    }
}