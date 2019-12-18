namespace SkbKontur.SqlStorageCore.Exceptions
{
    public class DatabaseUnavailableException : SqlStorageException
    {
        public DatabaseUnavailableException(string dbName, string host, int port)
            : base($"Could not establish connection to database {dbName} on {host}:{port}")
        {
        }
    }
}