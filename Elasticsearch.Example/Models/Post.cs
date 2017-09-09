using System;
using System.Collections.Generic;
using Nest;

namespace Elasticsearch.Example.Models
{
    public class Post
    {        
        public string Id { get; set; }
        
        public DateTime? CreationDate { get; set; }
        
        public int? Score { get; set; }
        
        public int? AnswerCount { get; set; }
        
        public string Body { get; set; }
        
        public string Title { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public IEnumerable<string> Suggest { get; set; }
    }
}
