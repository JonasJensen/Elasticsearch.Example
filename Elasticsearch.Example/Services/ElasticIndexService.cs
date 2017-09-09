using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;
using System.Xml;
using System.Xml.Linq;
using Elasticsearch.Example.Models;
using Elasticsearch.Example.Utils;
using Nest;

namespace Elasticsearch.Example.Services
{
    public class ElasticIndexService
    {
        private readonly IElasticClient _client;

        public ElasticIndexService()
        {
            _client = ElasticConfig.GetClient();
        }

        public void CreateIndex()
        {
            if (!_client.IndexExists(ElasticConfig.IndexName).Exists)
            {
                var settings = GetIndexSettings();

                var indexDescriptor = new CreateIndexDescriptor(ElasticConfig.IndexName)
                    .Mappings(ms => ms
                        .Map<Post>(m => m
                            .Properties(props => props
                                .Text(t => t
                                    .Name(post => post.Title)
                                    .Analyzer("english")
                                )
                                .Text(t => t
                                    .Name(post => post.Body)
                                    .Analyzer("english")
                                )
                                .Keyword(k => k
                                    .Name(post => post.Tags)
                                )
                                .Completion(c => c
                                    .Name(post => post.Suggest)
                                )
                            )
                            .AutoMap()
                        )
                    )
                    .Settings(s => settings);

                _client.CreateIndex(ElasticConfig.IndexName, i => indexDescriptor);
            }

            BulkIndex(HostingEnvironment.MapPath("~/data/posts.xml"), 20000);
        }

        private static IndexSettingsDescriptor GetIndexSettings()
        {
            #region TokenFilters

            //We add the danish stop word and stemmer filters.
            var tokenFilters = new Dictionary<string, ITokenFilter>
            {
                {
                    "english_stop",
                    new StopTokenFilter
                    {
                        StopWords = "_english_"
                    }
                },
                {
                    "english_snow",
                    new SnowballTokenFilter()
                    {
                        Language = SnowballLanguage.English
                    }
                },
            };

            #endregion

            #region Analyzers

            //We create a custom analyzer to use in the index
            var analyzers = new Dictionary<string, IAnalyzer>
            {
                {
                    "english",
                    new CustomAnalyzer
                    {
                        CharFilter = new[] {"html_strip"},
                        Filter = new []{ "lowercase", "english_stop", "english_snow", "asciifolding" },
                        Tokenizer = "standard"
                    }
                },
                {
                    "default",
                    new CustomAnalyzer
                    {
                        CharFilter = new[] { "html_strip" },
                        Filter = new[] { "lowercase", "asciifolding" },
                        Tokenizer = "standard"
                    }
                }
            };

            #endregion

            var indexSettings = new IndexSettingsDescriptor();

            indexSettings.NumberOfShards(1);
            indexSettings.NumberOfReplicas(0);
            indexSettings.Analysis(d => new Analysis
            {
                TokenFilters = new TokenFilters(tokenFilters),
                Analyzers = new Analyzers(analyzers)
            });
            return indexSettings;
        }

        #region Load Date

        private static IEnumerable<Post> LoadPostsFromFile(string inputUrl)
        {
            using (var reader = XmlReader.Create(inputUrl))
            {
                reader.MoveToContent();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "row")
                    {
                        if (string.Equals(reader.GetAttribute("PostTypeId"), "1"))
                        {
                            var el = XNode.ReadFrom(reader) as XElement;

                            if (el != null)
                            {
                                var post = new Post
                                {
                                    Id = el.Attribute("Id").Value,
                                    Title = el.Attribute("Title") != null ? el.Attribute("Title").Value : "",
                                    CreationDate = DateTime.Parse(el.Attribute("CreationDate").Value),
                                    Score = int.Parse(el.Attribute("Score").Value),
                                    Body = el.Attribute("Body").Value,
                                    Tags =
                                        el.Attribute("Tags") != null
                                            ? el.Attribute("Tags")
                                                .Value.Replace("><", "|")
                                                .Replace("<", "")
                                                .Replace(">", "")
                                                .Replace("&gt;&lt;", "|")
                                                .Replace("&lt;", "")
                                                .Replace("&gt;", "")
                                                .Split('|')
                                            : null,
                                    AnswerCount =
                                        el.Attribute("AnswerCount") != null
                                            ? int.Parse(el.Attribute("AnswerCount").Value)
                                            : 0
                                };
                                post.Suggest = post.Tags;
                                yield return post;
                            }
                        }
                    }
                }
            }
        }

        private void BulkIndex(string path, int maxItems)
        {
            const int batch = 1000;
            foreach (var batches in LoadPostsFromFile(path).Take(maxItems).Batch(batch))
            {
                var result = _client.IndexMany<Post>(batches, ElasticConfig.IndexName);
            }
        }

        #endregion
    }
}