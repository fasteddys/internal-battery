using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Dto;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Helpers.Job;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Google.Apis.CloudTalentSolution.v3.Data;
using UpDiddyLib.Shared.GoogleJobs;
using Microsoft.AspNetCore.Http;
using UpDiddyLib.Domain.Models;
using Hangfire;
using Newtonsoft.Json.Linq;
using UpDiddyApi.Workflow;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyLib.Domain;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class SubscriberCourseService : ISubscriberCourseService
    {
        private readonly IServiceProvider _services;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private ISysEmail _sysEmail;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private IHangfireService _hangfireService;
        private readonly ICloudTalentService _cloudTalentService;
        private readonly UpDiddyDbContext _db = null;
        private readonly ILogger _syslog;
        private readonly IHttpClientFactory _httpClientFactory = null;
        private readonly ICompanyService _companyService;
        private readonly ISubscriberService _subscriberService;
        private readonly IMemoryCacheService _cache;
        private IAuthorizationService _authorizationService;
        private readonly IDistributedCache _redisCache;

        public SubscriberCourseService(IServiceProvider services, IHangfireService hangfireService, ICloudTalentService cloudTalentService)
        {
            _services = services;

            _db = _services.GetService<UpDiddyDbContext>();
            _syslog = _services.GetService<ILogger<JobService>>();
            _httpClientFactory = _services.GetService<IHttpClientFactory>();
            _repositoryWrapper = _services.GetService<IRepositoryWrapper>();
            _mapper = _services.GetService<IMapper>();
            _sysEmail = _services.GetService<ISysEmail>();
            _configuration = _services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            _companyService = services.GetService<ICompanyService>();
            _subscriberService = services.GetService<ISubscriberService>();
            _cache = services.GetService<IMemoryCacheService>();
            _hangfireService = hangfireService;
            _cloudTalentService = cloudTalentService;
            _authorizationService = services.GetService<IAuthorizationService>();
            _redisCache = services.GetService<IDistributedCache>();
        }

        public async Task<List<SubscriberCourseDto>> GetSubscriberCourses(Guid subscriberGuid, Guid talentGuid, int ExcludeActive, int ExcludeCompleted, bool isRecruiter)
        { 
            List<SubscriberCourseDto> rVal = null;
            
            if (Guid.Empty.Equals(talentGuid) || talentGuid == null)
                throw new NotFoundException("Talent guid must be specified");

            if (subscriberGuid == talentGuid || isRecruiter)            
                rVal = await _repositoryWrapper.StoredProcedureRepository.GetSubscriberCourses(talentGuid, ExcludeCompleted, ExcludeActive);            
            else
                throw new UnauthorizedAccessException();
            return  rVal;
        }


    }
}
