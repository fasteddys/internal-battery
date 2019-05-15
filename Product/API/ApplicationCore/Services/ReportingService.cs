using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
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
    }
}
