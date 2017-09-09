using System;
using System.Configuration;
using Nest;

namespace Elasticsearch.Example.Services
{
    public static class ElasticConfig
    {
        public static string IndexName => ConfigurationManager.AppSettings["indexName"];

        public static string ElastisearchUrl => ConfigurationManager.AppSettings["elastisearchUrl"];

        public static IElasticClient GetClient()
        {
            var node = new Uri("http://localhost:9200");
            var settings = new ConnectionSettings(node);
            settings.DefaultIndex("stackoverflow");
            return new ElasticClient(settings);
        }
    }
}