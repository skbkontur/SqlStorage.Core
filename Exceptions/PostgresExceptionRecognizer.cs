using JetBrains.Annotations;

using Npgsql;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Exceptions
{
    public static class PostgresExceptionRecognizer
    {
        private const string uniqueViolationCode = "23505";

        public static bool TryRecognizeException([NotNull] PostgresException postgresException, out SqlStorageException sqlStorageException)
        {
            switch (postgresException.SqlState)
            {
            case uniqueViolationCode:
                sqlStorageException = new UniqueViolationException(postgresException.MessageText, postgresException.ConstraintName, uniqueViolationCode, postgresException.Detail);
                return true;

            default:
                sqlStorageException = null;
                return false;
            }
        }
    }
}