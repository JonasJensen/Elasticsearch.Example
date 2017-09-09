using System.Collections.Generic;

namespace Elasticsearch.Example.Models
{
    public class SearchResult<T>
    {
        public int Total { get; set; }

        public int Page { get; set; }

        public IEnumerable<T> Results { get; set; }

        public long ElapsedMilliseconds { get; set; }

        public Dictionary<string, long> AggregationsByTags { get; set; }
    }
}