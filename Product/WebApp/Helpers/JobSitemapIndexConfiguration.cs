using Microsoft.AspNetCore.Mvc;
using SimpleMvcSitemap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

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
            return new SitemapNode(urlHelper.Action(source.JobPostingGuid.ToString(), "job")) { LastModificationDate = source.ModifyDate.HasValue ? source.ModifyDate.Value : source.CreateDate, ChangeFrequency = ChangeFrequency.Daily };
            
        }
    }
}
