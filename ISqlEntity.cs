namespace SKBKontur.Catalogue.EDI.SqlStorageCore
{
    public interface ISqlEntity<TKey>
    {
        TKey Id { get; set; }
    }
}