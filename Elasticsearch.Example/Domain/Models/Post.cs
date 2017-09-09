using System;
using System.Collections.Generic;

namespace Elasticsearch.Example.Domain.Models
{
    public class Post
    {        
        public string Id { get; set; }

        public string Body { get; set; }

        public string Title { get; set; }

        public DateTime? CreationDate { get; set; }
        
        public int? Score { get; set; }

        public User Poster { get; set; }

        public IEnumerable<Tag> Tags { get; set; }

        public IEnumerable<Answer> Answers { get; set; }
    }
}
