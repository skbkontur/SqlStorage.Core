namespace SkbKontur.SqlStorageCore
{
    public interface ISqlEntity<TKey> where TKey : notnull
    {
        TKey Id { get; set; }
    }
}