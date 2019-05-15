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
                topicNodes.Add(new SitemapNode(Url.Action(topic.Slug, "topics")) { LastModificationDate = topic.ModifyDate.HasValue ? topic.ModifyDate.Value : topic.CreateDate, ChangeFrequency = ChangeFrequency.Monthly });
            }
            return new SitemapProvider().CreateSitemap(new SitemapModel(topicNodes));
        }

        [HttpGet]
        [Route("[controller]/static-sitemap.xml")]
        public IActionResult Static()
        {
            List<SitemapNode> nodes = new List<SitemapNode>
            {
                new SitemapNode(Url.Action("Index","Home")) { ChangeFrequency = ChangeFrequency.Weekly, LastModificationDate = new DateTime(2019, 4, 11) },
                new SitemapNode(Url.Action("About","Home")) { ChangeFrequency = ChangeFrequency.Monthly, LastModificationDate = new DateTime(2019, 1, 1) },
                new SitemapNode(Url.Action("Offers","Home")){ ChangeFrequency = ChangeFrequency.Daily, LastModificationDate = new DateTime(2019, 4, 19) },
                new SitemapNode(Url.Action("Contact","Home")){ ChangeFrequency = ChangeFrequency.Monthly, LastModificationDate = new DateTime(2019, 1, 1) },
                new SitemapNode(Url.Action("Privacy","Home")){ ChangeFrequency = ChangeFrequency.Monthly, LastModificationDate = new DateTime(2019, 1, 1) },
                new SitemapNode(Url.Action("FAQ","Home")){ ChangeFrequency = ChangeFrequency.Monthly, LastModificationDate = new DateTime(2019, 1, 1) },
                new SitemapNode(Url.Action("TermsOfService","Home")){ ChangeFrequency = ChangeFrequency.Monthly, LastModificationDate = new DateTime(2019, 1, 1) },
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
                new SitemapIndexNode(Url.Action("jobs-sitemap.xml", "sitemap"))
                // todo: create separate node for Butter CMS sitemap (once we have published content there other than site nav and LPs) https://buttercms.com/docs/api/?csharp#feeds
            };

            return new SitemapProvider().CreateSitemapIndex(new SitemapIndexModel(sitemapIndexNodes));
        }
    }
}