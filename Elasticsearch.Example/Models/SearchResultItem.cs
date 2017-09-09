namespace Elasticsearch.Example.Models
{
    public class SearchResultItem<T>
    {
        public T Item { get; set; }
        public double Score { get; set; }

        public SearchResultItem(T item, double? score)
        {
            Item = item;
            Score = score ?? 0.0;
        }
    }
}