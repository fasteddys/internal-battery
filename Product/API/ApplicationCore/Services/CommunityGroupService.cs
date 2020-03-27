using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.Models;
using UpDiddyApi.Workflow;
using UpDiddyLib.Dto;
using UpDiddyLib.Dto.User;
using UpDiddyLib.Shared;
using UpDiddyLib.Helpers;
using System.Web;
using AutoMapper;
using System.Security.Claims;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyLib.Domain;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.Helpers;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class CommunityGroupService : ICommunityGroupService
    {
        private UpDiddyDbContext _db { get; set; }
        private IConfiguration _configuration { get; set; }
        private ICloudStorage _cloudStorage { get; set; }
        private ILogger _logger { get; set; }
        private IRepositoryWrapper _repository { get; set; }
        private readonly IMapper _mapper;
        private ITaggingService _taggingService { get; set; }
        private IHangfireService _hangfireService { get; set; }
        private IFileDownloadTrackerService _fileDownloadTrackerService { get; set; }
        private ISysEmail _sysEmail;
        private readonly IButterCMSService _butterCMSService;
        private readonly ZeroBounceApi _zeroBounceApi;


        public CommunityGroupService(UpDiddyDbContext context,
            IConfiguration configuration,
            ICloudStorage cloudStorage,
            IRepositoryWrapper repository,
            ILogger<SubscriberService> logger,
            IMapper mapper,
            ITaggingService taggingService,
            IHangfireService hangfireService,
            IFileDownloadTrackerService fileDownloadTrackerService,
            ISysEmail sysEmail,
            IButterCMSService butterCMSService)
        {
            _db = context;
            _configuration = configuration;
            _cloudStorage = cloudStorage;
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _taggingService = taggingService;
            _hangfireService = hangfireService;
            _fileDownloadTrackerService = fileDownloadTrackerService;
            _sysEmail = sysEmail;
            _zeroBounceApi = new ZeroBounceApi(_configuration, _repository, _logger);
            _butterCMSService = butterCMSService;
            
        }

        public async Task<CommunityGroup> GetCommunityGroupByName(string name)
        {
            return await _repository.CommunityGroupRepository.GetCommunityGroupByNameAsync(name);
        }

        public async Task<CommunityGroup> GetCommunityGroupByGuid(Guid communityGroupGuid)
        {
            return await _repository.CommunityGroupRepository.GetCommunityGroupByGuidAsync(communityGroupGuid);
        }

        public async Task<Guid> CreateCommunityGroup(UpDiddyLib.Domain.Models.CommunityGroupDto communityGroupDto)
        {
           
            try
            {

                var CommunityGroup = await GetCommunityGroupByGuid(communityGroupDto.CommunityGroupGuid);
                if (CommunityGroup != null)
                    throw new AlreadyExistsException($"CommunityGroupGuid {communityGroupDto.CommunityGroupGuid} already exists");

                CommunityGroup = await GetCommunityGroupByName(communityGroupDto.Name);
                if (CommunityGroup != null)
                    throw new AlreadyExistsException($"A community group already exists called {communityGroupDto.Name}");


                var communityGroupGuid = Guid.NewGuid();

                // create the user in the CareerCircle database
                await _repository.CommunityGroupRepository.Create(new CommunityGroup()
                {
                    CommunityGroupGuid = communityGroupGuid,
                    Name = communityGroupDto.Name,
                    CreateDate = DateTime.UtcNow,
                    CreateGuid = Guid.Empty,
                    IsDeleted = 0
                });


                await _repository.CommunityGroupRepository.SaveAsync();
                var communityGroup = _repository.CommunityGroupRepository.GetCommunityGroupByGuidAsync(communityGroupDto.CommunityGroupGuid);

                return communityGroupGuid;

            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"CommunityGroupService.CreateNewCommunityGroupAsync: An error occured while attempting to create a community group. Message: {e.Message}", e);
                throw (e);
            }
        }



        public async Task UpdateCommunityGroup(UpDiddyLib.Domain.Models.CommunityGroupDto communityGroupDto)
        {
            try
            {
                var CommunityGroup = await GetCommunityGroupByGuid(communityGroupDto.CommunityGroupGuid);
                if (CommunityGroup == null)
                    throw new NotFoundException($"CommunityGroupGUID {communityGroupDto.CommunityGroupGuid} does not exist exist");

                // update the user in the CareerCircle database
                CommunityGroup.Name = communityGroupDto.Name;
                CommunityGroup.ModifyDate = DateTime.UtcNow;

                await _repository.CommunityGroupRepository.SaveAsync();

            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"CommunityGroupService.UpdateCommunityGroupAsync: An error occured while attempting to create a community group. Message: {e.Message}", e);
                throw e;
            }
        }


        public async Task<bool> DeleteCommunityGroup(Guid communityGroupGuid)
        {
            try
            {
                var communityGroup = await GetCommunityGroupByGuid(communityGroupGuid);
                if (communityGroup == null)
                    throw new NotFoundException($"CommunityGroupGUID {communityGroupGuid} does not exist exist");

                communityGroup.IsDeleted = 1;
                 _repository.CommunityGroupRepository.Update(communityGroup);

                return true;

            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"CommunityGroupService.UpdateCommunityGroupAsync: An error occured while attempting to delete a community group. Message: {e.Message}", e);
                throw e;
            }
            return false;
        }



        public async Task<CommunityGroupSearchResultDto> SearchCommunityGroupsAsync(int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*")
        {
            DateTime startSearch = DateTime.Now;
            CommunityGroupSearchResultDto searchResults = new CommunityGroupSearchResultDto();

            // map descending to azure search sort syntax of "asc" or "desc"  default is ascending so only map descending 
            string orderBy = sort;
            if (order == "descending")
                orderBy = orderBy + " desc";
            List<String> orderByList = new List<string>();
            orderByList.Add(orderBy);

            var results = _repository.CommunityGroupRepository.GetAll()
                .Where(p=>p.Name.StartsWith(keyword))
                .Skip(offset)
                .Take(limit);

 

            DateTime startMap = DateTime.Now;
            searchResults.CommunityGroups = results
                .Select(s => new UpDiddyLib.Domain.Models.CommunityGroupDto()
                {
                     Name = s.Name,
                     CommunityGroupGuid = s.CommunityGroupGuid.Value
                })
                .ToList();
            
            searchResults.TotalHits = results.Count();
            searchResults.PageSize = limit;
            searchResults.NumPages = searchResults.PageSize != 0 ? (int)Math.Ceiling((double)searchResults.TotalHits / searchResults.PageSize) : 0;
            searchResults.CommunityGroupCount = searchResults.CommunityGroups.Count;
            searchResults.PageNum = (offset / limit) + 1;
            
            DateTime stopMap = DateTime.Now;

            // calculate search timing metrics 
            TimeSpan intervalTotalSearch = stopMap - startSearch;
            TimeSpan intervalSearchTime = startMap - startSearch;
            TimeSpan intervalMapTime = stopMap - startMap;

            // assign search metrics to search results 
            searchResults.SearchTimeInMilliseconds = intervalTotalSearch.TotalMilliseconds;
            searchResults.SearchQueryTimeInTicks = intervalSearchTime.Ticks;
            searchResults.SearchMappingTimeInTicks = intervalMapTime.Ticks;
            
            return searchResults;
        }

        public async Task<CommunityGroupSubscriber> GetCommunityGroupSubscriber(Guid communityGroupSubscriberGuid)
        {
            return await _repository.CommunityGroupSubscriberRepository.GetCommunityGroupSubscriberByGuid(communityGroupSubscriberGuid);

        }

        public async Task<List<Subscriber>> GetCommunityGroupSubscribers(Guid communityGroupGuid)
        {
            return await _repository.CommunityGroupSubscriberRepository.GetAllCommunityGroupSubscribers(communityGroupGuid).ToListAsync();

        }

        public async Task<Guid> CreateCommunityGroupSubscriber(CommunityGroupSubscriberDto communityGroupSubscriberDto)
        {
            try
            {

                var communityGroupSubscriber = await GetCommunityGroupSubscriber(communityGroupSubscriberDto.CommunityGroupSubscriberGuid);
                if (communityGroupSubscriber != null)
                    throw new AlreadyExistsException($"CommunityGroupSubscriberGuid {communityGroupSubscriberDto.CommunityGroupSubscriberGuid} already exists");

                var communityGroupSubscriberGuid = Guid.NewGuid();

                // create the user in the CareerCircle database
                await _repository.CommunityGroupSubscriberRepository.Create(new CommunityGroupSubscriber()
                {
                    CommunityGroupSubscriberGuid = communityGroupSubscriberGuid,
                    SubscriberId = communityGroupSubscriberDto.SubscriberId,
                    CommunityGroupId = communityGroupSubscriberDto.CommunityGroupId,
                    CreateDate = DateTime.UtcNow,
                    CreateGuid = Guid.Empty,
                    IsDeleted = 0
                });


                await _repository.CommunityGroupRepository.SaveAsync();
                var cgSubscriber = await _repository.CommunityGroupSubscriberRepository.GetCommunityGroupSubscriberByGuid(communityGroupSubscriberDto.CommunityGroupSubscriberGuid);

                return cgSubscriber.CommunityGroupSubscriberGuid.Value;

            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"CommunityGroupService.CreateNewCommunityGroupAsync: An error occured while attempting to create a community group. Message: {e.Message}", e);
                throw (e);
            }
        }

        //public Task UpdateCommunityGroupSubscriber(CommunityGroupSubscriber communityGroupSubscriber)
        //{

        //}

        public async Task<bool> DeleteCommunityGroupSubscriber(Guid communityGroupSubscriberGuid)
        {
            try
            {
                var communityGroupSubscription = await GetCommunityGroupSubscriber(communityGroupSubscriberGuid);
                if (communityGroupSubscription == null)
                    throw new NotFoundException($"CommunityGroupSubscriberGUID {communityGroupSubscriberGuid} does not exist exist");

                communityGroupSubscription.IsDeleted = 1;
                 _repository.CommunityGroupSubscriberRepository.Update(communityGroupSubscription);

                return true;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"CommunityGroupService.UpdateCommunityGroupAsync: An error occured while attempting to delete a community group. Message: {e.Message}", e);
                throw e;
            }
            return false;
        }

        public async Task<CommunityGroupSubscriberSearchResultDto> SearchCommunityGroupSubscribersAsync(Guid communityGroupGuid, int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*")
        {
            DateTime startSearch = DateTime.Now;
            CommunityGroupSubscriberSearchResultDto searchResults = new CommunityGroupSubscriberSearchResultDto();

            // map descending to azure search sort syntax of "asc" or "desc"  default is ascending so only map descending 
            string orderBy = sort;
            if (order == "descending")
                orderBy = orderBy + " desc";
            List<String> orderByList = new List<string>();
            orderByList.Add(orderBy);

            var results = _repository.CommunityGroupSubscriberRepository.GetAllCommunityGroupSubscribers(communityGroupGuid)
                .Where(p => p.LastName.StartsWith(keyword) || p.FirstName.StartsWith(keyword))
                .Skip(offset)
                .Take(limit);



            DateTime startMap = DateTime.Now;
            searchResults.CommunityGroupSubscribers = results
                .Select(s => new UpDiddyLib.Domain.Models.SubscriberDto()
                {
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    SubscriberGuid = s.SubscriberGuid.Value
                })
                .ToList();

            searchResults.TotalHits = results.Count();
            searchResults.PageSize = limit;
            searchResults.NumPages = searchResults.PageSize != 0 ? (int)Math.Ceiling((double)searchResults.TotalHits / searchResults.PageSize) : 0;
            searchResults.CommunityGroupSubscriberCount = searchResults.CommunityGroupSubscribers.Count;
            searchResults.PageNum = (offset / limit) + 1;

            DateTime stopMap = DateTime.Now;

            // calculate search timing metrics 
            TimeSpan intervalTotalSearch = stopMap - startSearch;
            TimeSpan intervalSearchTime = startMap - startSearch;
            TimeSpan intervalMapTime = stopMap - startMap;

            // assign search metrics to search results 
            searchResults.SearchTimeInMilliseconds = intervalTotalSearch.TotalMilliseconds;
            searchResults.SearchQueryTimeInTicks = intervalSearchTime.Ticks;
            searchResults.SearchMappingTimeInTicks = intervalMapTime.Ticks;

            return searchResults;
        }
    }
}