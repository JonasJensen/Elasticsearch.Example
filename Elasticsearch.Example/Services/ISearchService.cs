using System.Collections.Generic;
using Elasticsearch.Example.Models;

namespace Elasticsearch.Example.Services
{
    public interface ISearchService<T>
    {
        SearchResult<T> Search(string query, int page = 1, int pageSize = 10);

        IEnumerable<string> Autocomplete(string query);

        SearchResult<Post> SearchByCategory(string query, IEnumerable<string> tags, int page, int pageSize);
    }
}