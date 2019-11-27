using JetBrains.Annotations;

using Npgsql;

namespace SkbKontur.SqlStorageCore.Exceptions
{
    public static class PostgresExceptionRecognizer
    {
        public static bool TryRecognizeException([NotNull] PostgresException postgresException, out SqlStorageException sqlStorageException)
        {
            switch (postgresException.SqlState)
            {
            case "23502":
                sqlStorageException = new NotNullViolationException(postgresException.MessageText, postgresException.ColumnName);
                return true;
            case "23505":
                sqlStorageException = new UniqueViolationException(postgresException.MessageText, postgresException.ConstraintName, postgresException.Detail);
                return true;

            default:
                sqlStorageException = null;
                return false;
            }
        }
    }
}