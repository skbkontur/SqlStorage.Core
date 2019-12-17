namespace SkbKontur.SqlStorageCore.Exceptions
{
    public sealed class UniqueViolationException : SqlStorageException
    {
        public UniqueViolationException(string message, string constraint, string details)
            : base(message)
        {
            Constraint = constraint;
            Details = details;
        }

        public string Constraint { get; }

        public string Details { get; }
    }
}