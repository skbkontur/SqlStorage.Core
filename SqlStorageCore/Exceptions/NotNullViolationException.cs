namespace SkbKontur.SqlStorageCore.Exceptions
{
    public sealed class NotNullViolationException : SqlStorageException
    {
        public NotNullViolationException(string message, string columnName)
            : base(message)
        {
            ColumnName = columnName;
        }

        public string ColumnName { get; }
    }
}