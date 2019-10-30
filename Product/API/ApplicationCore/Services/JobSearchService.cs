using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.User;
using UpDiddyLib.Helpers;
using UpDiddyApi.Helpers.Job;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Services
{
    public class JobSearchService : IJobSearchService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ICloudTalentService _cloudTalentService;
        private readonly ISubscriberService _subscriberService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public JobSearchService(UpDiddyDbContext _db
        , IRepositoryWrapper repositoryWrapper
        , IMapper mapper
        , ILogger<JobSearchService> sysLog
        , IConfiguration configuration
        , IHttpClientFactory httpClientFactory
        , ISubscriberService subscriberService
        , ICloudTalentService cloudTalentService)
        {
            _repositoryWrapper = repositoryWrapper;
            _configuration = configuration;
            _mapper = mapper;
            _cloudTalentService = cloudTalentService;

        }


        public async Task<int> GetActiveJobCount()
        {
            return await _repositoryWrapper.JobPosting.GetAll().Where(jp => jp.IsDeleted == 0).CountAsync();
        }

        public async Task<List<UpDiddyLib.Domain.Models.JobPostingDto>> GetSimilarJobs(Guid jobPostingGuid, int limit, int offset, string sort, string order)
        {
            var job = await _repositoryWrapper.JobPosting.GetJobPostingByGuid(jobPostingGuid);

            JobQueryDto jobQuery = JobQueryHelper.CreateJobQueryForSimilarJobs(job.Province, job.City, job.Title, limit);
            jobQuery.PageSize = limit;
            JobSearchResultDto jobSearchForSingleJob = _cloudTalentService.JobSearch(jobQuery);

            // If jobs in same city come back less than 6, broaden search to state.
            if (jobSearchForSingleJob.JobCount < limit)
            {
                jobQuery = JobQueryHelper.CreateJobQueryForSimilarJobs(job.Province, string.Empty, job.Title, limit);
                jobSearchForSingleJob = _cloudTalentService.JobSearch(jobQuery);
            }

            var jobPostingDto = _mapper.Map<List<JobViewDto>, List<UpDiddyLib.Domain.Models.JobPostingDto>>(jobSearchForSingleJob.Jobs);
            return jobPostingDto;
        }
    }
}
