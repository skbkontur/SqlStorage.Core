namespace SkbKontur.SqlStorageCore
{
    public interface ISqlEntity<TKey>
    {
        TKey Id { get; set; }
    }
}