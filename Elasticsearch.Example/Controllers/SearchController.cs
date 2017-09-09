using System.Collections.Generic;
using System.Web.Http;
using Elasticsearch.Example.Services;

namespace Elasticsearch.Example.Controllers
{
    [RoutePrefix("api")]
    public class SearchController : ApiController
    {
        private readonly ElasticSearchService service;

        public SearchController()
        {
            service = new ElasticSearchService();
        }

        [HttpGet]
        [Route("search")]
        public IHttpActionResult Search(string q, int page = 1, int pageSize = 10)
        {
            
            var results = service.Search(q, page, pageSize);
            return Ok(results);
        }

        [HttpGet]
        [Route("index")]
        public IHttpActionResult Index()
        {
            var indexService = new ElasticIndexService();
            indexService.CreateIndex();
            return Ok();
        }

        [HttpGet]
        [Route("autocomplete")]
        public IHttpActionResult Autocomplete(string q)
        {
            return Ok(service.Autocomplete(q));
        }

        [HttpPost]
        [Route("searchbycategory")]
        public IHttpActionResult SearchByCategory([FromBody]dynamic json)
        {
            string q = json.q;
            var categories = (IEnumerable<string>)json.categories.ToObject<List<string>>();
            return Ok(service.SearchByCategory(q, categories, 1, 10));
        }
        
    }
}
