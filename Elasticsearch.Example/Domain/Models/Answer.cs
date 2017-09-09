using System;

namespace Elasticsearch.Example.Domain.Models
{
    public class Answer
    {
        public string Id { get; set; }

        public string Body { get; set; }

        public int? Score { get; set; }

        public User Poster { get; set; }

        public DateTime? CreationDate { get; set; }
    }
}