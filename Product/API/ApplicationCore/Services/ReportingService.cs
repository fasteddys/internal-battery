using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.Reporting;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class ReportingService : IReportingService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        public ReportingService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }
        public async Task<List<JobApplicationCountDto>> GetApplicationCountByCompanyAsync(ODataQueryOptions<JobApplication> options, Guid? companyGuid)
        {
            //get all jobs querayble
            var queryable = options.ApplyTo(await _repositoryWrapper.JobApplication.GetAllJobApplicationsAsync());
            var jobPostingRep = await _repositoryWrapper.JobPosting.GetAllJobPostings();
            //get all companies queryable
            var companyRep = await _repositoryWrapper.Company.GetAllCompanies();

            var jobApplicationsQuery = from jobApplication in queryable.Cast<JobApplication>()
                join jp in jobPostingRep on jobApplication.JobPostingId equals jp.JobPostingId
                join company in companyRep on jp.CompanyId equals company.CompanyId
                where !companyGuid.HasValue || company.CompanyGuid == companyGuid.Value
                group new {
                    CompanyName = company.CompanyName,
                    CompanyGuid = company.CompanyGuid
                } by jp.CompanyId into g
                select new JobApplicationCountDto{
                    CompanyName = g.First().CompanyName,
                    CompanyGuid = g.First().CompanyGuid,
                    ApplicationCount = g.Count()
                };
            var list = await jobApplicationsQuery.ToListAsync();
            return list;
        }

        /// <summary>
        /// Get active/published job counts per company and posted date range.
        /// </summary>
        /// <param name="startPostDate"></param>
        /// <param name="endPostDate"></param>
        /// <returns>A list of DTOs representing a job posting per company by date report.</returns>
        public async Task<List<JobPostingCountReportDto>> GetActiveJobPostCountPerCompanyByDates(DateTime? startPostDate, DateTime? endPostDate)
        {
            List<JobPostingCountReportDto> jobPostingCountReportDtos;

            //get all jobs querayble
            var jobPostingRepo = _repositoryWrapper.JobPosting.GetAllJobPostings().Result;

            //get all companies queryable
            var companyRepo = _repositoryWrapper.Company.GetAllCompanies().Result;

            var query = from c in companyRepo
                        join jp in jobPostingRepo on c.CompanyId equals jp.CompanyId
                        where jp.JobStatus == (int)JobPostingStatus.Active
                            && c.IsJobPoster == 1
                            && (startPostDate == null || (jp.PostingDateUTC.Date >= startPostDate))
                            && (endPostDate == null || (jp.PostingDateUTC.Date <= endPostDate))
                        group new { c.CompanyName, jp.PostingDateUTC } 
                        by new { c.CompanyName, jp.PostingDateUTC.Date } into g
                        orderby g.Key.CompanyName, g.Key.Date
                        select new JobPostingCountReportDto
                        {
                            CompanyName = g.Key.CompanyName,
                            PostingDate = g.Key.Date,
                            PostingCount = g.Select(x => x.PostingDateUTC.Date).Count()
                        };

            jobPostingCountReportDtos = await query.ToListAsync();

            return jobPostingCountReportDtos;
        }
    }
}
