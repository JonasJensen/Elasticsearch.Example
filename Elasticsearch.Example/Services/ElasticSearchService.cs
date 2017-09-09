using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Example.Models;
using Nest;

namespace Elasticsearch.Example.Services
{
    public class ElasticSearchService : ISearchService<Post>
    {
        private readonly IElasticClient _client;

        public ElasticSearchService()
        {
            _client = ElasticConfig.GetClient();
        }

        public SearchResult<Post> Search(string query, int page, int pageSize)
        {
            var fieldQuery = new MultiMatchQuery
            {
                Query = query,
                Fields = new[]
                {
                    Infer.Field<Post>(p => p.Title),
                    Infer.Field<Post>(p => p.Body),
                    Infer.Field<Post>(p => p.Tags)
                },
                Fuzziness = Fuzziness.EditDistance(1)
            };
            
            var searchRequest = new SearchRequest
            {
                From = page - 1,
                Size = pageSize,
                Query = new BoolQuery
                {
                    Must = new QueryContainer[]
                    {
                        fieldQuery
                    }
                },
                Aggregations = new TermsAggregation("by_tags")
                {
                    Field = Infer.Field<Post>(p => p.Tags),
                    Size = 10
                }
            };

            var result = _client.Search<Post>(searchRequest);
            
            return new SearchResult<Post>
            {
                Total = (int)result.Total,
                Page = page,
                Results = result.Documents,
                ElapsedMilliseconds = result.Took,
                AggregationsByTags = result.Aggs.Terms("by_tags").Buckets.ToDictionary(x => x.Key, y => y.DocCount.GetValueOrDefault(0))
            };
        }

        public SearchResult<Post> SearchByCategory(string query, IEnumerable<string> tags, int page = 1,
            int pageSize = 10)
        {

            var fieldQuery = new MultiMatchQuery
            {
                Query = query,
                Fields = new[]
                {
                    Infer.Field<Post>(p => p.Title),
                    Infer.Field<Post>(p => p.Body),
                    Infer.Field<Post>(p => p.Tags)
                },
                Fuzziness = Fuzziness.EditDistance(1)
            };

            var categoryFilter = new BoolQuery
            {
                Must = new QueryContainer[]
                {
                    new TermsQuery
                    {
                        Field = Infer.Field<Post>(p => p.Tags),
                        Terms = tags
                    }
                }
            };
            
            var searchRequest = new SearchRequest
            {
                From = page - 1,
                Size = pageSize,
                Query = new BoolQuery
                {
                    Must = new QueryContainer[]
                    {
                        fieldQuery
                    },
                    Filter = new QueryContainer[]
                    {
                        categoryFilter
                    }
                },
                Aggregations = new TermsAggregation("by_tags")
                {
                    Field = Infer.Field<Post>(p => p.Tags),
                    Size = 10
                }
            };

            var result = _client.Search<Post>(searchRequest);

            return new SearchResult<Post>
            {
                Total = (int)result.Total,
                Page = page,
                Results = result.Documents,
                ElapsedMilliseconds = result.Took,
                AggregationsByTags = result.Aggs.Terms("by_tags").Buckets.ToDictionary(x => x.Key, y => y.DocCount.GetValueOrDefault(0))
            };
        }

        public IEnumerable<string> Autocomplete(string query)
        {
            var searchRequest = new SearchRequest
            {
                 Suggest = new SuggestContainer
                {
                    {
                        "tag-suggestions",
                        new SuggestBucket
                        {
                            Prefix = query,
                            Completion = new CompletionSuggester
                            {
                                Field = Infer.Field<Post>(p => p.Suggest),
                                Size = 1
                            }
                        }
                    }
                }
            };

            var result = _client.Search<Post>(searchRequest);

            return result.Suggest["tag-suggestions"].SelectMany(x => x.Options).Select(y => y.Text);
        }
    }
}