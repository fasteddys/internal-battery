using System;
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
using UpDiddyLib.Dto;

namespace UpDiddy.Controllers
{
    public class SitemapController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;


        public SitemapController(IApi api,
            IConfiguration configuration,
            IHostingEnvironment env)
            : base(api)
        {
            _env = env;
            _configuration = configuration;
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
        [Route("/sitemap.xml")]
        public IActionResult Sitemap()
        {
            List<SitemapIndexNode> sitemapIndexNodes = new List<SitemapIndexNode>
            {
                new SitemapIndexNode(Url.Action("static-sitemap.xml", "sitemap")),
                new SitemapIndexNode(Url.Action("courses-sitemap.xml", "sitemap")),
                new SitemapIndexNode(Url.Action("jobs-sitemap.xml", "sitemap")),
                new SitemapIndexNode(Url.Action("browse-jobs.xml", "sitemap"))
                // todo: create separate node for Butter CMS sitemap (once we have published content there other than site nav and LPs) https://buttercms.com/docs/api/?csharp#feeds
            };

            return new SitemapProvider().CreateSitemapIndex(new SitemapIndexModel(sitemapIndexNodes));
        }

        [HttpGet]
        [Route("[controller]/browse-jobs.xml")]
        public async Task<IActionResult> BrowseJobs()
        {
            List<SitemapIndexNode> sitemapIndexNodes = new List<SitemapIndexNode>
            {
                new SitemapIndexNode(Url.Action("browse-jobs-location/country.xml", "sitemap")),
            };

            JobQueryBuilder jobQueryBuilder = new JobQueryBuilder(_Api);
            jobQueryBuilder.Country = "us";
            var response = jobQueryBuilder.Execute();



            return new SitemapProvider().CreateSitemapIndex(new SitemapIndexModel(sitemapIndexNodes));

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
            nodes.Add(new SitemapNode(string.Format("{0}/{1}", relUrl.TrimStart('/'), urlPart.TrimStart('/'))) { ChangeFrequency = ChangeFrequency.Hourly });

            int jobCount = 0;
            int page = 1;
            while (jobCount < 5000 && jobCount < response.TotalHits)
            {
                nodes.Add(new SitemapNode(string.Format("{0}/{1}/{2}", relUrl.TrimStart('/'), urlPart.TrimStart('/'), page)) { ChangeFrequency = ChangeFrequency.Hourly });
                jobCount += response.PageSize;
                page++;
            }

            if(nextFacet == null || response.Facets.FirstOrDefault(x => x.Name == nextFacet.Key) == null)
                return new SitemapProvider().CreateSitemap(new SitemapModel(nodes));

            foreach (var item in response.Facets.FirstOrDefault(x => x.Name == nextFacet.Key).Facets)
            {
                nodes.Add(new SitemapNode(string.Format("/sitemap/{0}/{1}/{2}.xml", relUrl.TrimStart('/'), urlPart.TrimStart('/'), nextFacet.func(item.Label))) { ChangeFrequency = ChangeFrequency.Hourly });
            }

            return new SitemapProvider().CreateSitemap(new SitemapModel(nodes));
        }

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
        [Route("[controller]/browse-jobs-industry/{category}/{industry}.xml")]
        [Route("[controller]/browse-jobs-industry/{industry}/{category}/us.xml")]
        [Route("[controller]/browse-jobs-industry/{industry}/{category}/us/{state}.xml")]
        [Route("[controller]/browse-jobs-industry/{industry}/{category}/us/{state}/{city}.xml")]
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
    }
}