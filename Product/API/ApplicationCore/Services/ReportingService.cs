using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
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
    }
}
