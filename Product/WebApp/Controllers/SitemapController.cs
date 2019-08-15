using Microsoft.VisualBasic;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SimpleMvcSitemap;
using UpDiddy.Api;
using UpDiddy.Helpers;
using UpDiddy.Helpers.Job;
using UpDiddy.Services;
using UpDiddy.Services.ButterCMS;
using UpDiddyLib.Dto;
using System.Xml;
using UpDiddyLib.Helpers;
using System.IO;

namespace UpDiddy.Controllers
{
    public class SitemapController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;
        private readonly ICacheService _cacheService;
        private readonly IButterCMSService _butterService;


        public SitemapController(IApi api,
            IConfiguration configuration,
            ICacheService cacheService,
            IButterCMSService butterCMSService,
            IHostingEnvironment env)
            : base(api)
        {
            _env = env;
            _configuration = configuration;
            _cacheService = cacheService;
            _butterService = butterCMSService;
        }

        [HttpGet]
        [Route("[controller]/courses-sitemap.xml")]
        public async Task<IActionResult> Courses()
        {
            // course detail pages cannot be crawled by Google because they require a subscriber exist. 
            // for now, just list the topics the topics - these pages contain the urls for courses (better than nothing)
            IList<TopicDto> topics = await _Api.TopicsAsync();
            List<SitemapNode> topicNodes = new List<SitemapNode>();
            foreach (var topic in topics)
            {
                topicNodes.Add(new SitemapNode(Url.Action(topic.Slug, "topics")) { ChangeFrequency = ChangeFrequency.Monthly });
            }
            return new SitemapProvider().CreateSitemap(new SitemapModel(topicNodes));
        }

        [HttpGet]
        [Route("[controller]/static-sitemap.xml")]
        public IActionResult Static()
        {
            List<SitemapNode> nodes = new List<SitemapNode>
            {
                new SitemapNode(Url.Action("Index","Home")) { ChangeFrequency = ChangeFrequency.Weekly },
                new SitemapNode(Url.Action("About","Home")) { ChangeFrequency = ChangeFrequency.Monthly },
                new SitemapNode(Url.Action("Offers","Home")){ ChangeFrequency = ChangeFrequency.Daily },
                new SitemapNode(Url.Action("Contact","Home")){ ChangeFrequency = ChangeFrequency.Monthly },
                new SitemapNode(Url.Action("Privacy","Home")){ ChangeFrequency = ChangeFrequency.Monthly },
                new SitemapNode(Url.Action("FAQ","Home")){ ChangeFrequency = ChangeFrequency.Monthly },
                new SitemapNode(Url.Action("TermsOfService","Home")){ ChangeFrequency = ChangeFrequency.Monthly },
            };
            return new SitemapProvider().CreateSitemap(new SitemapModel(nodes));
        }

        [HttpGet]
        [Route("[controller]/jobs-sitemap.xml")]
        public async Task<IActionResult> Jobs(int? currentPage)
        {

            List<JobPostingDto> jobs = await _Api.GetAllJobsAsync();
            var jobSitemapIndexConfiguration = new JobSitemapIndexConfiguration(jobs.AsQueryable(), currentPage, Url);
            return new DynamicSitemapIndexProvider().CreateSitemapIndex(new SitemapProvider(), jobSitemapIndexConfiguration);
        }

        [HttpGet]
        [Route("[controller]/cms-sitemap.xml")]
        public async Task<IActionResult> Cms(){
            XmlDocument cmsXmlResponse = await _butterService.GetButterSitemapAsync();
            XmlNode node = cmsXmlResponse.FirstChild.NextSibling;
            bool NextNodeExists = true; 
            List<SitemapNode> nodes = new List<SitemapNode>();

            // Add static pages for Blog route
            nodes.Add(new SitemapNode(Url.Action("Index","Blog")) { ChangeFrequency = ChangeFrequency.Weekly });
            nodes.Add(new SitemapNode(Url.Action("Search","Blog")) { ChangeFrequency = ChangeFrequency.Weekly });


            while(NextNodeExists){
                string BlogPostUrl = _configuration["Environment:BaseUrl"] + "Blog/Post/" + node.InnerText;
                nodes.Add(new SitemapNode(BlogPostUrl) { ChangeFrequency = ChangeFrequency.Weekly });
                NextNodeExists = node.NextSibling != null;
                node = node.NextSibling;
            }

            IList<string> AuthorSlugs = await _butterService.GetBlogAuthorSlugsAsync();
            foreach(string Slug in AuthorSlugs){
                string AuthorPageUrl = _configuration["Environment:BaseUrl"] + "Blog/Author/" + Slug;
                nodes.Add(new SitemapNode(AuthorPageUrl) { ChangeFrequency = ChangeFrequency.Weekly });
            }

            IList<string> CategorySlugs = await _butterService.GetBlogCategorySlugsAsync();
            foreach(string Slug in CategorySlugs){
                string CategoryPageUrl = _configuration["Environment:BaseUrl"] + "Blog/Category/" + Slug;
                nodes.Add(new SitemapNode(CategoryPageUrl) { ChangeFrequency = ChangeFrequency.Weekly });
            }

            IList<string> TagSlugs = await _butterService.GetBlogTagSlugsAsync();
            foreach(string Slug in TagSlugs){
                string TagPageUrl = _configuration["Environment:BaseUrl"] + "Blog/Tag/" + Slug;
                nodes.Add(new SitemapNode(TagPageUrl) { ChangeFrequency = ChangeFrequency.Weekly });
            }

            int NumberOfBlogPages = await _butterService.GetNumberOfBlogPostPagesAsync();
            string BlogIndexPaginationUrl = _configuration["Environment:BaseUrl"] + "Blog/";
            for(int i = 1; i <= NumberOfBlogPages; i++){
                nodes.Add(new SitemapNode(BlogIndexPaginationUrl + i) { ChangeFrequency = ChangeFrequency.Weekly });
            }

            return new SitemapProvider().CreateSitemap(new SitemapModel(nodes));
        }

        [HttpGet]
        [Route("/sitemap.xml")]
        public async Task<IActionResult> Sitemap()
        {

            List<SitemapIndexNode> sitemapIndexNodes = new List<SitemapIndexNode>
            {
                new SitemapIndexNode(Url.Action("static-sitemap.xml", "sitemap")),
                new SitemapIndexNode(Url.Action("courses-sitemap.xml", "sitemap")),
                new SitemapIndexNode(Url.Action("jobs-sitemap.xml", "sitemap")),
                new SitemapIndexNode(Url.Action("cms-sitemap.xml", "sitemap"))
            };
            
            // add browse jobs
            sitemapIndexNodes.AddRange(await BrowseJobsLocation());
            sitemapIndexNodes.AddRange(await BrowseJobsIndustry());
            sitemapIndexNodes.AddRange(await BrowseJobsCategory());

            return new SitemapProvider().CreateSitemapIndex(new SitemapIndexModel(sitemapIndexNodes));
        }

        private async Task<List<SitemapIndexNode>> BrowseJobsLocation()
        {
            List<SitemapIndexNode> sitemapIndexNodes = new List<SitemapIndexNode>
            {
                new SitemapIndexNode("/sitemap/browse-jobs-location/country.xml"),
            };

            List<Facet> facets = new List<Facet>()
            {
                new Facet() { Key = "country", Value = "us" },
                Facet.StateFacet(),
                Facet.CityFacet(),
                Facet.IndustryFacet(),
                Facet.CategoryFacet()
            };

            var nodes = await _cacheService.GetSetCachedValueAsync("browse-jobs-location-sitemapindex",
                async () => await BuildBrowseSitemapIndex("browse-jobs-location", facets), DateTimeOffset.Now.AddDays(1));

            sitemapIndexNodes.AddRange(nodes);
            return sitemapIndexNodes;
        }

        private async Task<List<SitemapIndexNode>> BrowseJobsIndustry()
        {
            List<SitemapIndexNode> sitemapIndexNodes = new List<SitemapIndexNode>();

            List<Facet> facets = new List<Facet>()
            {
                Facet.IndustryFacet(),
                Facet.CategoryFacet(),
                new Facet() { Key = "country", Value = "us" },
                Facet.StateFacet(),
                Facet.CityFacet(),
            };

            var nodes = await _cacheService.GetSetCachedValueAsync("browse-jobs-industry-sitemapindex",
                async () => await BuildBrowseSitemapIndex("browse-jobs-industry", facets), DateTimeOffset.Now.AddDays(1));

            sitemapIndexNodes.AddRange(nodes);
            return sitemapIndexNodes;
        }

        private async Task<List<SitemapIndexNode>> BrowseJobsCategory()
        {
            List<SitemapIndexNode> sitemapIndexNodes = new List<SitemapIndexNode>();
            List<Facet> facets = new List<Facet>()
            {
                Facet.CategoryFacet(),
                Facet.IndustryFacet(),
                new Facet() { Key = "country", Value = "us" },
                Facet.StateFacet(),
                Facet.CityFacet(),
            };

            var nodes = await _cacheService.GetSetCachedValueAsync("browse-jobs-category-sitemapindex",
                async () => await BuildBrowseSitemapIndex("browse-jobs-category", facets), DateTimeOffset.Now.AddDays(1));

            sitemapIndexNodes.AddRange(nodes);
            return sitemapIndexNodes;
        }

        private async Task<List<SitemapIndexNode>> BuildBrowseSitemapIndex(string baseUrl, List<Facet> facets)
        {
            List<SitemapIndexNode> sitemapIndexNodes = new List<SitemapIndexNode>();
            sitemapIndexNodes.AddRange(await BuildBrowseSitemapIndex(baseUrl, facets, 0));
            return sitemapIndexNodes;
        }

        /// <summary>
        /// Recursively build sitemapindex references
        /// </summary>
        /// <param name="baseUrl">string starting url</param>
        /// <param name="facets">Facets[] ordered</param>
        /// <param name="index">int level in the facet</param>
        /// <returns></returns>
        private async Task<List<SitemapIndexNode>> BuildBrowseSitemapIndex(string baseUrl, List<Facet> facets, int index)
        {
            List<SitemapIndexNode> nodes = new List<SitemapIndexNode>();
            JobQueryBuilder jobQueryBuilder = new JobQueryBuilder(_Api);
            string urlPart = baseUrl.TrimEnd('/');
            if (index > facets.Count)
                return nodes;

            List<Facet> treeFacets = new List<Facet>(); // deep copy facets
            foreach(var facet in facets)
            {
                treeFacets.Add(new Facet() { Key = facet.Key, Value = facet.Value, func = facet.func });
            }

            for(int i = 0; i <= index; i++)
            {
                urlPart += string.Format("/{0}", facets[i].Value);
                jobQueryBuilder.AddFacet(facets[i]);
            }
            var response = await jobQueryBuilder.Execute();

            var nextFacet = index < facets.Count ? facets[index] : null;

            if (nextFacet.Key == "country")
                nodes.AddRange(await BuildBrowseSitemapIndex(baseUrl, treeFacets, index + 1));

            if (nextFacet == null || response.Facets.FirstOrDefault(x => x.Name == nextFacet.Key) == null)
                return nodes;

            foreach(var item in response.Facets.FirstOrDefault(x => x.Name == nextFacet.Key).Facets)
            {
                nodes.Add(new SitemapIndexNode(string.Format("sitemap/{0}/{1}.xml", urlPart.TrimEnd('/'), nextFacet.func(item.Label))));
                if (treeFacets.Count > index)
                    treeFacets[index].Value = item.Label;

                // traverse only if necessary
                if(index + 1 < facets.Count && (facets[index + 1].Key == "country" ||response.Facets.FirstOrDefault(x => x.Name == facets[index + 1].Key) != null))
                    nodes.AddRange(await BuildBrowseSitemapIndex(baseUrl, treeFacets, index + 1));
            }
            return nodes;
        }

        private async Task<ActionResult> BuildBrowseSitemap(string relUrl, List<Facet> facets, JobQueryBuilder foo, Facet nextFacet = null)
        {
            var response = await foo.Execute();
            string urlPart = "";
            foreach(Facet facet in facets)
            {
                var containerFacet = response.Facets.FirstOrDefault(x => x.Name == facet.Key);
                urlPart += string.Format("/{0}", facet.Value);

                if (facet.Key == "country" && facet.Value == "us")
                    continue;

                if (containerFacet == null  || containerFacet.Facets.FirstOrDefault(x => facet.func(x.Label).Equals(facet.Value, StringComparison.InvariantCultureIgnoreCase)) == null)
                    return NotFound();
            }

            List<SitemapNode> nodes = new List<SitemapNode>();
            // add base url
            nodes.Add(new SitemapNode(string.Format("{0}/{1}", relUrl.TrimStart('/'), urlPart.TrimStart('/'))) { ChangeFrequency = ChangeFrequency.Daily });

            int jobCount = 0;
            int page = 1;
            while (jobCount < 5000 && jobCount < response.TotalHits)
            {
                nodes.Add(new SitemapNode(string.Format("{0}/{1}/{2}", relUrl.TrimStart('/'), urlPart.TrimStart('/'), page)) { ChangeFrequency = ChangeFrequency.Hourly });
                jobCount += response.PageSize;
                page++;
            }

            return new SitemapProvider().CreateSitemap(new SitemapModel(nodes));
        }

        #region Browse Job Sitemaps
        [HttpGet]
        [Route("[controller]/browse-jobs-location/country.xml")]
        [Route("[controller]/browse-jobs-location/us/{state}.xml")]
        [Route("[controller]/browse-jobs-location/us/{state}/{city}.xml")]
        [Route("[controller]/browse-jobs-location/us/{state}/{city}/{industry}.xml")]
        [Route("[controller]/browse-jobs-location/us/{state}/{city}/{industry}/{category}.xml")]
        public async Task<IActionResult> BrowseJobsLocation(string state, string city, string industry, string category)
        {
            List<SitemapNode> nodes = new List<SitemapNode>();

            List<Facet> facets = new List<Facet>();
            Facet nextFacet = Facet.StateFacet();
            var jobQueryBuilder = new JobQueryBuilder(_Api);
            // start tree at country
            jobQueryBuilder.Country = "us";
            facets.Add(new Facet() { Key = "country", Value = "us" });

            if(state != null)
            {
                jobQueryBuilder.State = state;
                facets.Add(Facet.StateFacet(state));
                nextFacet = Facet.CityFacet();
            }

            if(city != null)
            {
                jobQueryBuilder.City = city;
                facets.Add(Facet.CityFacet(city));
                nextFacet = Facet.IndustryFacet();
            }

            if(industry != null)
            {
                jobQueryBuilder.Industry = industry;
                facets.Add(Facet.IndustryFacet(industry));
                nextFacet = Facet.CategoryFacet();
            }

            if(category != null)
            {
                jobQueryBuilder.Category = category;
                facets.Add(Facet.CategoryFacet(category));
            }

            return await BuildBrowseSitemap("/browse-jobs-location", facets, jobQueryBuilder, nextFacet);
        }

        [HttpGet]
        [Route("[controller]/browse-jobs-industry/{industry}.xml")]
        [Route("[controller]/browse-jobs-industry/{industry}/{category}.xml")]
        [Route("[controller]/browse-jobs-industry/{industry}/{category}/us.xml")]
        [Route("[controller]/browse-jobs-industry/{industry}/{category}/us/{state}.xml")]
        [Route("[controller]/browse-jobs-industry/{industry}/{category}/us/{state}/{city}.xml")]
        public async Task<IActionResult> BrowseJobsIndustry(string industry, string category, string state, string city)
        {
            List<SitemapNode> nodes = new List<SitemapNode>();

            List<Facet> facets = new List<Facet>();
            Facet nextFacet = Facet.CategoryFacet();
            var jobQueryBuilder = new JobQueryBuilder(_Api);
            // start tree at country
            jobQueryBuilder.Industry = industry;
            if (industry != null)
            {
                jobQueryBuilder.Industry = industry;
                facets.Add(Facet.IndustryFacet(industry));
                nextFacet = Facet.CategoryFacet();
            }

            if (category != null)
            {
                jobQueryBuilder.Category = category;
                facets.Add(Facet.CategoryFacet(category));
                nextFacet = new Facet() { Key = "country", Value = "us" };
            }

            if((category != null && state == null) || state != null)
            {
                jobQueryBuilder.Country = "us";
                facets.Add(new Facet() { Key = "country", Value = "us" });
                nextFacet = Facet.StateFacet();
            }

            if (state != null)
            {
                jobQueryBuilder.State = state;
                facets.Add(Facet.StateFacet(state));
                nextFacet = Facet.CityFacet();
            }

            if (city != null)
            {
                jobQueryBuilder.City = city;
                facets.Add(Facet.CityFacet(city));
                nextFacet = null;
            }

            return await BuildBrowseSitemap("/browse-jobs-industry", facets, jobQueryBuilder, nextFacet);
        }

        [HttpGet]
        [Route("[controller]/browse-jobs-category/{category}.xml")]
        [Route("[controller]/browse-jobs-category/{category}/{industry}.xml")]
        [Route("[controller]/browse-jobs-category/{industry}/{category}/us.xml")]
        [Route("[controller]/browse-jobs-category/{industry}/{category}/us/{state}.xml")]
        [Route("[controller]/browse-jobs-category/{industry}/{category}/us/{state}/{city}.xml")]
        public async Task<IActionResult> BrowseJobsCategory(string category, string industry, string state, string city)
        {
            List<SitemapNode> nodes = new List<SitemapNode>();

            List<Facet> facets = new List<Facet>();
            Facet nextFacet = Facet.IndustryFacet();
            var jobQueryBuilder = new JobQueryBuilder(_Api);
            // start tree at country
            jobQueryBuilder.Industry = industry;
            if (category != null)
            {
                jobQueryBuilder.Category = category;
                facets.Add(Facet.CategoryFacet(category));
                nextFacet = Facet.IndustryFacet();
            }

            if (industry != null)
            {
                jobQueryBuilder.Industry = industry;
                facets.Add(Facet.IndustryFacet(industry));
                nextFacet = new Facet() { Key = "country", Value = "us" };
            }


            if((industry != null && state == null) || state != null)
            {
                jobQueryBuilder.Country = "us";
                facets.Add(new Facet() { Key = "country", Value = "us" });
                nextFacet = Facet.StateFacet();
            }

            if (state != null)
            {
                jobQueryBuilder.State = state;
                facets.Add(Facet.StateFacet(state));
                nextFacet = Facet.CityFacet();
            }

            if (city != null)
            {
                jobQueryBuilder.City = city;
                facets.Add(Facet.CityFacet(city));
                nextFacet = null;
            }

            return await BuildBrowseSitemap("/browse-jobs-industry", facets, jobQueryBuilder, nextFacet);
        }
        #endregion
    }
}