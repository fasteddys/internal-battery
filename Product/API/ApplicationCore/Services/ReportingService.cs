using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.Reporting;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class ReportingService : IReportingService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        public ReportingService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }
        public async Task<List<JobApplicationCountDto>> GetApplicationCountPerCompanyByDates(Guid? companyGuid, DateTime? startDate, DateTime? endDate)
        {
            //get all jobs querayble
            var jobPostingRep = _repositoryWrapper.JobPosting.GetAllJobPostings().Result;

            //get all companies queryable
            var companyRep = _repositoryWrapper.Company.GetAllCompanies().Result;

            var jobApplicationsQuery = (from jobPosting in jobPostingRep
                                                     join company in companyRep on jobPosting.CompanyId equals company.CompanyId
                                                     select new
                                                     {
                                                         CompanyId=jobPosting.CompanyId,
                                                         CompanyGuid = company.CompanyGuid,
                                                         CompanyName = company.CompanyName,
                                                         JobPostingId = jobPosting.JobPostingId,
                                                         PostingDate = jobPosting.PostingDateUTC
                                                     });




            List<JobApplicationCountDto> jobApplicationqueryWithWhereClause;

            //apply where clause
            if (companyGuid != null)
            {
                jobApplicationqueryWithWhereClause = await jobApplicationsQuery.Where(q => q.CompanyGuid == companyGuid
                                                                  && (startDate == null || (q.PostingDate >= startDate))
                                                                  && (endDate == null || (q.PostingDate <= endDate)))
                                                                  .GroupBy(x => x.CompanyId)
                                                                  .Select(x => new JobApplicationCountDto {
                                                                      CompanyGuid = x.First().CompanyGuid,
                                                                      CompanyName = x.First().CompanyName,
                                                                      ApplicationCount = x.Select(y => y.JobPostingId).Count()
                                                                  }).ToListAsync();
            }
            else
            {
                jobApplicationqueryWithWhereClause = await jobApplicationsQuery.Where(q => (startDate == null || (q.PostingDate >= startDate))
                                                                   && (endDate == null || (q.PostingDate <= endDate)))
                                                                   .GroupBy(x => x.CompanyId)
                                                                   .Select(x => new JobApplicationCountDto
                                                                   {
                                                                       CompanyGuid = x.First().CompanyGuid,
                                                                       CompanyName = x.First().CompanyName,
                                                                       ApplicationCount = x.Select(y => y.JobPostingId).Count()
                                                                   }).ToListAsync(); ;
            }

            return jobApplicationqueryWithWhereClause;
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
                        group new { c, jp } by new { c.CompanyName, jp.PostingDateUTC.Date } into g
                        orderby g.Key.CompanyName, g.Key.Date
                        select new JobPostingCountReportDto
                        {
                            CompanyName = g.Key.CompanyName,
                            PostingDate = g.Key.Date,
                            PostingCount = g.Select(x => x.jp.PostingDateUTC.Date).Count()
                        };

            jobPostingCountReportDtos = await query.ToListAsync();

            return jobPostingCountReportDtos;
        }
    }
}
