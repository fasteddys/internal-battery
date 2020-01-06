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
    public class TalentService : ITalentService
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

        public TalentService(IServiceProvider services, IHangfireService hangfireService, ICloudTalentService cloudTalentService)
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


        public async Task<List<TalentSearchOrderByDto>> GetTalentSearchOrderBy()
        {
            List<TalentSearchOrderByDto> rVal = new List<TalentSearchOrderByDto>();

            TalentSearchOrderByDto orderBy = new TalentSearchOrderByDto()
            {
                Name = "Date Modified ↓",
                Value = "update_date desc"
            };
            rVal.Add(orderBy);

            orderBy = new TalentSearchOrderByDto()
            {
                Name = "Join Date ↓",
                Value = "create_date desc"
            };
            rVal.Add(orderBy);

            orderBy = new TalentSearchOrderByDto()
            {
                Name = "First Name ↓",
                Value = "first_name desc"
            };
            rVal.Add(orderBy);


            orderBy = new TalentSearchOrderByDto()
            {
                Name = "First Name ↑",
                Value = "first_name desc"
            };
            rVal.Add(orderBy);


            orderBy = new TalentSearchOrderByDto()
            {
                Name = "Last Name ↓",
                Value = "last_name desc"
            };
            rVal.Add(orderBy);


            orderBy = new TalentSearchOrderByDto()
            {
                Name = "Last Name ↑",
                Value = "last_name"
            };
            rVal.Add(orderBy);
 
            return rVal;
        }


        public async Task<List<TalentSearchPartnersFilterDto>> GetTalentSearchPartnersFilter()
        {

            List<TalentSearchPartnersFilterDto> rval = new List<TalentSearchPartnersFilterDto>();

            List<SubscriberSourceStatisticDto> SourceStats = _db.SubscriberSources.ProjectTo<SubscriberSourceStatisticDto>(_mapper.ConfigurationProvider).ToList();
            foreach ( SubscriberSourceStatisticDto stat in SourceStats)
            {
                TalentSearchPartnersFilterDto filter = new TalentSearchPartnersFilterDto()
                {
                    Name = stat.Name + "(" + stat.Count.ToString() + ")",
                    Value = stat.Name

                };
                rval.Add(filter);

            }

            return rval;
        }



        public async Task<ProfileSearchResultDto> SearchTalent(int limit, int offset, string orderBy, string keyword, string location, string partner)
        {
            int PageNum = 0;
            if ( limit != 0 )
                PageNum = offset / limit;
            ProfileQueryDto profileQueryDto = new ProfileQueryDto()
            {
                Keywords = keyword != null ? keyword : string.Empty,
                Location = location != null ? location : string.Empty,
                SourcePartner = partner != null ? partner : string.Empty,
                OrderBy = orderBy != null ? orderBy : string.Empty,
                PageSize = limit,
                PageNum = PageNum


            };

            ProfileSearchResultDto rVal = _cloudTalentService.ProfileSearch(profileQueryDto);
            return rVal;

        }




        public async Task<UpDiddyLib.Dto.SubscriberDto> TalentDetails(Guid subscriberGuid, Guid talentGuid, bool isRecruiter)
        {
            
            // Validate guid for GetSubscriber call
            if (Guid.Empty.Equals(talentGuid) || talentGuid == null)
                throw new NotFoundException("Talent guid must be specified");



            if (subscriberGuid == talentGuid || isRecruiter)
            {

                UpDiddyLib.Dto.SubscriberDto subscriberDto = await SubscriberFactory.GetSubscriber(_repositoryWrapper, subscriberGuid, _syslog, _mapper);

                if (subscriberDto == null)
                    throw new NotFoundException("Subscriber does not exist");

                // track the subscriber action if performed by someone other than the user who owns the file
                if (subscriberGuid != talentGuid)
                    new SubscriberActionFactory(_repositoryWrapper, _db, _configuration, _syslog, _redisCache).TrackSubscriberAction(subscriberGuid, "View subscriber", "Subscriber", subscriberDto.SubscriberGuid);

                return subscriberDto;
            }
            else
                throw new UnauthorizedAccessException();



        }

    }
}
