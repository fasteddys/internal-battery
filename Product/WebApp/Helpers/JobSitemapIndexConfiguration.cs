using Microsoft.AspNetCore.Mvc;
using SimpleMvcSitemap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;

namespace UpDiddy.Helpers
{
    public class JobSitemapIndexConfiguration : SitemapIndexConfiguration<JobPostingDto>
    {
        private readonly IUrlHelper urlHelper;

        public JobSitemapIndexConfiguration(IQueryable<JobPostingDto> dataSource, int? currentPage, IUrlHelper urlHelper)
            :base(dataSource, currentPage)
        {
            this.urlHelper = urlHelper;
        }

        public override SitemapIndexNode CreateSitemapIndexNode(int currentPage)
        {
            return new SitemapIndexNode(urlHelper.Action("jobs-sitemap.xml", "sitemap", new { currentPage }));
        }
        public override SitemapNode CreateNode(JobPostingDto source)
        {
            // this line can be removed once semantic job path is built using automapper
            source.SemanticJobPath = Utils.CreateSemanticJobPath(source.Industry?.Name, source.JobCategory?.Name, source.Country, source.Province, source.City, source.JobPostingGuid.ToString());

            return new SitemapNode(source.SemanticJobPath) { ChangeFrequency = ChangeFrequency.Hourly };
        }
    }
}
