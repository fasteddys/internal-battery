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

        public async Task<List<UpDiddyLib.Domain.Models.JobPostingDto>> GetSimilarJobs(Guid jobPostingGuid)
        {
            var job = await _repositoryWrapper.JobPosting.GetJobPostingByGuid(jobPostingGuid);

            JobQueryDto jobQuery = JobQueryHelper.CreateJobQueryForSimilarJobs(job.Province, job.City, job.Title, Int32.Parse(_configuration["CloudTalent:MaxNumOfSimilarJobsToBeReturned"]));
            JobSearchResultDto jobSearchForSingleJob = _cloudTalentService.JobSearch(jobQuery);

            if (jobSearchForSingleJob.JobCount < Int32.Parse(_configuration["CloudTalent:MaxNumOfSimilarJobsToBeReturned"]))
            {
                jobQuery = JobQueryHelper.CreateJobQueryForSimilarJobs(job.Province, string.Empty, job.Title, Int32.Parse(_configuration["CloudTalent:MaxNumOfSimilarJobsToBeReturned"]));
                jobSearchForSingleJob = _cloudTalentService.JobSearch(jobQuery);
            }

            var jobPostingDto = _mapper.Map<List<JobViewDto>, List<UpDiddyLib.Domain.Models.JobPostingDto>>(jobSearchForSingleJob.Jobs);
            return jobPostingDto;
        }

        public async Task<List<StateMapDto>> GetStateMapData()
        {
            var jobCount = await _repositoryWrapper.StoredProcedureRepository.GetJobCountPerProvince();
            var provinces = jobCount.Select(x => x.Province).Distinct();
            var jobCountDto = new List<StateMapDto>();
            Enums.ProvinceName stateNameEnum;
            Enums.ProvincePrefix statePrefixEnum;
            foreach (var province in provinces)
            {
                var str = province.Trim().Replace(" ", "").ToUpper();
                string statePrefix = string.Empty;
                if (Enum.TryParse(str, out statePrefixEnum))
                {
                    statePrefix = str;
                }
                else if (Enum.TryParse(str, out stateNameEnum))
                {
                    Enums.ProvinceName value = (Enums.ProvinceName)(int)stateNameEnum;
                    statePrefix = value.ToString();
                }
                if (!String.IsNullOrEmpty(statePrefix))
                {
                    var distinctCompanies = jobCount
                    .Where(x => x.Province == province)
                    .Select(m => new { m.CompanyName, m.CompanyGuid, m.Count })
                    .OrderByDescending(x => x.Count);
                    List<StateMapCompanyDto> companyCountdto = new List<StateMapCompanyDto>();
                    foreach (var company in distinctCompanies)
                    {
                        companyCountdto.Add(new StateMapCompanyDto()
                        {
                            CompanyGuid = company.CompanyGuid,
                            CompanyName = company.CompanyName,
                            JobCount = company.Count
                        });
                    }

                    if (companyCountdto.Count > 0)
                    {
                        var total = jobCount.Where(x => x.Province == province).Sum(s => s.Count);
                        jobCountDto.Add(new StateMapDto(statePrefix, companyCountdto, total));
                    }
                }
            }
            return jobCountDto;
        }
    }
}
