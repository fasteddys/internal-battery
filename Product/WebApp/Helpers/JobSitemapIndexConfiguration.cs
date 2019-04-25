using Microsoft.AspNetCore.Mvc;
using SimpleMvcSitemap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddy.Helpers
{
    public class JobSitemapIndexConfiguration : SitemapIndexConfiguration<JobViewDto>
    {
        private readonly IUrlHelper urlHelper;

        public JobSitemapIndexConfiguration(IQueryable<JobViewDto> dataSource, int? currentPage, IUrlHelper urlHelper)
            :base(dataSource, currentPage)
        {
            this.urlHelper = urlHelper;
        }

        public override SitemapIndexNode CreateSitemapIndexNode(int currentPage)
        {
            return new SitemapIndexNode(urlHelper.Action("jobs-sitemap.xml", "sitemap", new { currentPage }));
        }
        public override SitemapNode CreateNode(JobViewDto source)
        {
            return new SitemapNode(urlHelper.Action(source.JobPostingGuid.ToString(), "jobs")) { LastModificationDate = source.ModifyDate, ChangeFrequency = ChangeFrequency.Daily };
            
        }
    }
}
