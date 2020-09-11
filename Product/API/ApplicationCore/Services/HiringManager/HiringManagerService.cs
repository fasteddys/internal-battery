using AutoMapper;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Business.HiringManager;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Services.Identity;
using UpDiddyApi.ApplicationCore.Services.Identity.Interfaces;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.B2B;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
using UpDiddyLib.Domain.AzureSearchDocuments;

namespace UpDiddyApi.ApplicationCore.Services.HiringManager
{
    public class HiringManagerService : IHiringManagerService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly ILogger _logger;
        private readonly IHangfireService _hangfireService;
        private readonly IHubSpotService _hubspotService;
        private readonly ISubscriberService _subscriberService;

        public HiringManagerService(
            IConfiguration configuration,
            IRepositoryWrapper repositoryWrapper,
            IMapper mapper,
            IUserService userService,
            ILogger<HiringManagerService> logger,
            IHangfireService hangfireService,
            IHubSpotService hubspotService,
            ISubscriberService subscriberService)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _configuration = configuration;
            _userService = userService;
            _logger = logger;
            _hangfireService = hangfireService;
            _hubspotService = hubspotService;
            _subscriberService = subscriberService;
        }

        public async Task<HiringManagerDto> GetHiringManagerBySubscriberGuid(Guid subscriberGuid)
        {
            if (subscriberGuid == Guid.Empty)
                throw new FailedValidationException($"HiringManagerService:GetHiringManagerBySubscriberGuid subscriber guid cannot be empty({subscriberGuid})");


            // validate the subscriber is valid
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);

            if (subscriber == null)
                throw new FailedValidationException($"HiringManagerService:GetHiringManagerBySubscriberGuid Cannot locate subscriber {subscriberGuid}");

            try
            {
                var hiringManagerEntity = await _repositoryWrapper.HiringManagerRepository.GetHiringManagerBySubscriberId(subscriber.SubscriberId);

                if (hiringManagerEntity == null)
                {
                    throw new FailedValidationException($"HiringManagerService:GetHiringManagerBySubscriberGuid Cannot locate hiring manager for subscriber: {subscriberGuid}");
                }

                //map hiring manage entity to dto
                return _mapper.Map<HiringManagerDto>(hiringManagerEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError($"HiringManagerService:GetHiringManagerBySubscriberGuid  Error: {ex.ToString()} ");
                throw;
            }
        }

        public async Task<HiringManagerCandidateProfileDto> GetCandidateProfileDetail(Guid candidateProfileGuid)
        {
            if (candidateProfileGuid == Guid.Empty)
                throw new FailedValidationException($"HiringManagerService:GetCandidateProfileDetail candidate profile guid cannot be empty({candidateProfileGuid})");

            //validate profile
            var profiles = await _repositoryWrapper.ProfileRepository.GetProfilesByGuidList(new List<Guid> { candidateProfileGuid });

            if (profiles == null || profiles.Count == 0)
                throw new FailedValidationException($"HiringManagerService:GetHiringManagerBySubscriberGuid Cannot locate profile {candidateProfileGuid}");

            var profile = profiles.First();

            if (profile.Subscriber == null)
                throw new FailedValidationException($"HiringManagerService:GetHiringManagerBySubscriberGuid Cannot locate subscriber for profile {candidateProfileGuid}");

            try
            {
                if (String.IsNullOrWhiteSpace(profile.Subscriber.Title))
                {
                    var subscriberWorkHistory = await _repositoryWrapper.SubscriberWorkHistoryRepository.GetLastEmploymentDetailBySubscriberGuid(profile.Subscriber.SubscriberGuid.Value);

                    if (subscriberWorkHistory != null)
                    {
                        //map entity to dto
                        var hiringManagerCandidateProfileDto = _mapper.Map<HiringManagerCandidateProfileDto>(subscriberWorkHistory);
                        //cannot be mapped in the mapper
                        hiringManagerCandidateProfileDto.ProfileGuid = candidateProfileGuid;
                        return hiringManagerCandidateProfileDto;
                    }
                }

                return _mapper.Map<HiringManagerCandidateProfileDto>(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError($"HiringManagerService:GetCandidateProfileDetail  Error: {ex.ToString()} ");
                throw;
            }
        }

        public async Task<EducationalHistoryDto> GetCandidateEducationHistory(Guid candidateProfileGuid, int limit, int offset, string sort, string order)
        {
            if (candidateProfileGuid == Guid.Empty)
                throw new FailedValidationException($"HiringManagerService:GetCandidateEducationHistory candidate profile guid cannot be empty({candidateProfileGuid})");

            //validate profile
            var profiles = await _repositoryWrapper.ProfileRepository.GetProfilesByGuidList(new List<Guid> { candidateProfileGuid });

            if (profiles == null || profiles.Count == 0)
                throw new FailedValidationException($"HiringManagerService:GetCandidateEducationHistory Cannot locate profile {candidateProfileGuid}");

            var profile = profiles.First();

            //validate subscriber
            if (profile.Subscriber == null)
                throw new FailedValidationException($"HiringManagerService:GetCandidateEducationHistory Cannot locate subscriber for profile {candidateProfileGuid}");

            EducationalHistoryDto educationalHistoryDto = null;
            try
            {
                var educationHistoryEntity = await _repositoryWrapper.SubscriberEducationHistoryRepository.GetEducationalHistoryBySubscriberGuid(profile.Subscriber.SubscriberGuid.Value, limit, offset, sort, order);
                //map entity to dto
                educationalHistoryDto = _mapper.Map<EducationalHistoryDto>(educationHistoryEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError($"HiringManagerService:GetCandidateEducationHistory  Error: {ex.ToString()} ");
                throw;
            }

            return educationalHistoryDto;
        }

        public async Task<EmploymentHistoryDto> GetCandidateWorkHistory(Guid candidateProfileGuid, int limit, int offset, string sort, string order)
        {
            if (candidateProfileGuid == Guid.Empty)
                throw new FailedValidationException($"HiringManagerService:GetCandidateWorkHistory candidate profile guid cannot be empty({candidateProfileGuid})");

            //validate profile
            var profiles = await _repositoryWrapper.ProfileRepository.GetProfilesByGuidList(new List<Guid> { candidateProfileGuid });

            if (profiles == null || profiles.Count == 0)
                throw new FailedValidationException($"HiringManagerService:GetCandidateWorkHistory Cannot locate profile {candidateProfileGuid}");

            var profile = profiles.First();

            if (profile.Subscriber == null)
                throw new FailedValidationException($"HiringManagerService:GetCandidateWorkHistory Cannot locate subscriber for profile {candidateProfileGuid}");


            EmploymentHistoryDto employmentHistoryDto = null;
            try
            {
                var employmentHistoryEntity = await _repositoryWrapper.SubscriberWorkHistoryRepository.GetWorkHistoryBySubscriberGuid(profile.Subscriber.SubscriberGuid.Value, limit, offset, sort, order);
                //map entity to dto
                employmentHistoryDto = _mapper.Map<EmploymentHistoryDto>(employmentHistoryEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError($"HiringManagerService:GetCandidateWorkHistory  Error: {ex.ToString()} ");
                throw;
            }

            return employmentHistoryDto;
        }

        public async Task<SkillListDto> GetCandidateSkills(Guid candidateProfileGuid, int limit, int offset, string sort, string order)
        {
            if (candidateProfileGuid == Guid.Empty)
                throw new FailedValidationException($"HiringManagerService:GetCandidateSkills candidate profile guid cannot be empty({candidateProfileGuid})");

            //validate profile
            var profiles = await _repositoryWrapper.ProfileRepository.GetProfilesByGuidList(new List<Guid> { candidateProfileGuid });

            if (profiles == null || profiles.Count == 0)
                throw new FailedValidationException($"HiringManagerService:GetHiringManagerBySubscriberGuid Cannot locate profile {candidateProfileGuid}");

            var profile = profiles.First();

            if (profile.Subscriber == null)
                throw new FailedValidationException($"HiringManagerService:GetHiringManagerBySubscriberGuid Cannot locate subscriber for profile {candidateProfileGuid}");


            SkillListDto skillListDto = null;
            try
            {
                var candidateSkills = await _repositoryWrapper.SkillRepository.GetSkillsBySubscriberGuidSortedandPaged(profile.Subscriber.SubscriberGuid.Value, limit, offset, sort, order);

                //map entity to dto
                skillListDto = _mapper.Map<SkillListDto>(candidateSkills);
            }
            catch (Exception ex)
            {
                _logger.LogError($"HiringManagerService:GetCandidateSkills  Error: {ex.ToString()} ");
                throw;
            }

            return skillListDto;
        }

        public async Task UpdateHiringManager(Guid subscriberGuid, HiringManagerDto hiringManager)
        {
            _logger.LogInformation($"HiringManagerService:UpdateHiringManager  Starting for subscriber {subscriberGuid} ");
            if (subscriberGuid == Guid.Empty)
                throw new FailedValidationException($"HiringManagerService:UpdateHiringManager subscriber guid cannot be empty({subscriberGuid})");

            if (hiringManager == null)
                throw new FailedValidationException($"HiringManagerService:UpdateHiringManager HiringManagerDto cannot be null");

            // validate the subscriber is valid
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);

            if (subscriber == null)
                throw new FailedValidationException($"HiringManagerService:UpdateHiringManager Cannot locate subscriber {subscriberGuid}");

            var hiringManagerEntity = await _repositoryWrapper.HiringManagerRepository.GetHiringManagerBySubscriberId(subscriber.SubscriberId);

            if (hiringManagerEntity == null) throw new FailedValidationException($"HiringManagerService:UpdateHiringManager {subscriberGuid} is not a hiring manager");

            //update the subscriber and company record for the HM in DB
            try
            {
                await _repositoryWrapper.HiringManagerRepository.UpdateHiringManager(subscriber.SubscriberId, hiringManager);
                await _hubspotService.AddOrUpdateContactBySubscriberGuid(subscriberGuid, null, true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"HiringManagerService:UpdateHiringManager  Error: {ex.ToString()} ");
            }

            _logger.LogInformation($"HiringManagerService:UpdateHiringManager  Done for Hiring Manager with subscriber: {subscriberGuid} ");

        }

        public async Task<bool> AddHiringManager(Guid subscriberGuid, bool nonBlocking = true)
        {
            _logger.LogInformation($"HiringManagerService:AddHiringManager  Starting for subscriber {subscriberGuid} ");
            // validate the subscriber is valid
            var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberByGuidAsync(subscriberGuid);
            if (subscriber == null)
                throw new FailedValidationException($"HiringManagerService:AddHiringManager Cannot locate subscriber {subscriberGuid}");

            try
            {
                //check if hiring manager for subscriberid exists
                var hiringManager = await _repositoryWrapper.HiringManagerRepository.GetHiringManagerBySubscriberId(subscriber.SubscriberId);

                if (hiringManager == null)
                    await _repositoryWrapper.HiringManagerRepository.AddHiringManager(subscriber.SubscriberId);

                if (nonBlocking)
                {
                    _logger.LogInformation($"HiringManagerService:AddHiringManager : Background job starting for subscriber {subscriberGuid}");
                    _hangfireService.Enqueue<HiringManagerService>(j => j._AddHiringManager(subscriber));
                    return true;
                }
                else
                {
                    _logger.LogInformation($"HiringManagerService:AddHiringManager : awaiting _AddHiringManager for subscriber {subscriberGuid}");
                    await _AddHiringManager(subscriber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"HiringManagerService:AddHiringManager  Error: {ex.ToString()} ");
            }
            _logger.LogInformation($"HiringManagerService:AddHiringManager  Done for subscriber {subscriberGuid} ");

            return true;
        }

        public async Task<CandidateDetailDto> GetCandidate360Detail(Guid profileGuid)
        {
            var candidateDetailDto =  await _repositoryWrapper.HiringManagerRepository
                .GetCandidate360Detail(profileGuid);

            var subscriberVideo = await _subscriberService
                .GetVideoSasForHiringManager(profileGuid);

            if (subscriberVideo != null)
            {
                candidateDetailDto.IntroVideoUri = subscriberVideo.VideoURI;
                candidateDetailDto.IntroVideoThumbnailUri = subscriberVideo.VideoThumbnailURI;
            }

            return candidateDetailDto;
        }

        #region Candidate Search
        public async Task<HiringManagerCandidateSearchDto> CandidateSearchByHiringManagerAsync(Guid subscriberGuid, CandidateSearchQueryDto searchDto)
        {
            _logger.LogInformation($"HiringManagerService:CandidateSearchByHiringManagerAsync  Starting for subscriber {subscriberGuid} ");
            if(subscriberGuid.Equals(Guid.Empty))
                throw new FailedValidationException($"HiringManagerService:CandidateSearchByHiringManagerAsync subscriber guid cannot be empty({subscriberGuid})");
            if(searchDto == null)
                throw new FailedValidationException($"HiringManagerService:CandidateSearchByHiringManagerAsync searchDto cannot be null");

            try
            {
                // validate the the user provides a city if they also provided a radius
                if ((searchDto.CityGuid == null || searchDto.CityGuid == Guid.Empty) && searchDto.Radius != 0)
                    throw new FailedValidationException("A city guid must be provided if a radius is specifed");

                // All hiring managers are to use the default CC search company
                string CCCompanySearchGuid = _configuration["AzureSearch:CCCompanySearchGuid"];
                List<Guid> companyGuids = new List<Guid>()
                {
                    Guid.Parse(CCCompanySearchGuid)
                };

                HiringManagerCandidateSearchDto searchResults = null;

                // handle case of non geo search 
                if (searchDto.CityGuid == null || searchDto.CityGuid == Guid.Empty)
                {
                    searchResults = await _PerformAzureSearch(searchDto);
                    // Obfucscate the results 
                    //Obfuscate(results);
                    return searchResults;
                }


                // pick a random postal code for the city to get the last and long 
                Postal postal = _repositoryWrapper.PostalRepository.GetAll()
                    .Include(c => c.City)
                    .Where(p => p.City.CityGuid == searchDto.CityGuid && p.IsDeleted == 0)
                    .FirstOrDefault();

                // validate that the city has a postal code 
                if (postal == null)
                    throw new NotFoundException($"A city with an Guid of {searchDto.CityGuid} cannot be found.");

                searchResults = await _PerformAzureSearch(searchDto, (double)postal.Latitude, (double)postal.Longitude);
            }
            catch(Exception ex)
            {
                _logger.LogError($"HiringManagerService:CandidateSearchByHiringManagerAsync  Error: {ex.ToString()} ");
            }
            _logger.LogInformation($"HiringManagerService:CandidateSearchByHiringManagerAsync  Done");
            return null;
        }
        #endregion

        public async Task<bool> _AddHiringManager(Subscriber subscriber)
        {
            _logger.LogInformation($"HiringManagerService:_AddHiringManager  Starting for subscriber {subscriber.SubscriberGuid} ");
            var getUserResponse = await _userService.GetUserByEmailAsync(subscriber.Email);
            if (!getUserResponse.Success || string.IsNullOrWhiteSpace(getUserResponse.User.UserId))
                throw new ApplicationException("User could not be found in Auth0");
            _logger.LogInformation($"HiringManagerService:_AddHiringManager  Calling user service for  {getUserResponse.User.UserId} ");
            _userService.AssignRoleToUserAsync(getUserResponse.User.UserId, Role.HiringManager);
            _logger.LogInformation($"HiringManagerService:_AddHiringManager  Done");

            return true;
        }

        public Task<List<string>> GetProhibitiedEmailDomains()
            => _repositoryWrapper.ProhibitiedEmailDomainRepository.GetAll()
                .Where(e => e.IsDeleted == 0)
                .Select(e => e.Value)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync();

        private async Task<HiringManagerCandidateSearchDto> _PerformAzureSearch(CandidateSearchQueryDto searchDto, double lat = 0, double lng = 0)
        {

            try
            {
               // _logger.LogInformation($"HiringManagerService:_PerformAzureSearch: starting searchField ={searchFields} sort={searchDto.Sort} order={searchDto.Order} keyword={searchDto.Keyword} limit = {searchDto.Limit} radius={searchDto.Radius}");
               // if (companyGuids == null || companyGuids.Count == 0)
                   // throw new FailedValidationException("HiringManagerService:_PerformAzureSearch: Recruiter is not associated with the CareerCircle search company");

                DateTime startSearch = DateTime.Now;
                HiringManagerCandidateSearchDto searchResults = new HiringManagerCandidateSearchDto();

                string searchServiceName = _configuration["AzureSearch:SearchServiceName"];
                string adminApiKey = _configuration["AzureSearch:SearchServiceQueryApiKey"];
                string g2IndexName = _configuration["AzureSearch:CandidateIndexName"];

                // map descending to azure search sort syntax of "asc" or "desc"  default is ascending so only map descending 
                string orderBy = searchDto.Sort;
                if (searchDto.Order == "descending")
                    orderBy = orderBy + " desc";
                List<String> orderByList = new List<string>();
                orderByList.Add(orderBy);

                SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));
                ISearchIndexClient indexClient = serviceClient.Indexes.GetClient(g2IndexName);

                SearchParameters parameters;
                DocumentSearchResult<CandidateSDOC> results;

                parameters =
                    new SearchParameters()
                    {
                        Top = searchDto.Limit,
                        Skip = searchDto.Offset,
                        OrderBy = orderByList,
                        IncludeTotalResultCount = true,
                        // Add facets
                        Facets = new List<String>() { "IsResumeUploaded", "Title", "Skills", "WorkPreferences", "Personalities", "VideoUrl" },
                        //HasVideoInterview (does not exist), "Training" (facetable = false)
                    };

                // add search field if one is specified 
                //if (searchFields != null)
                //{
                //    parameters.SearchFields = searchFields;
                //}

                //(first name AND title) AND (at least one skill OR at least one work preference OR desired rate OR at least one training OR at least one language OR at least one education OR experience summary)
                // base filter
                // eliminates records with certain missing data 
                parameters.Filter = $"FirstName ne null and Tile ne null and (Skills/any() or WorkPreferences/any() or DesiredRate ne null or Training/any() or Languages/any() or ExperienceSummary ne null or Education/any())";

                //filters
                double radiusKm = 0;
                // check to see if radius is in play
                if (searchDto.Radius > 0)
                {

                    radiusKm = searchDto.Radius * 1.60934;
                    if (lat == 0)
                        throw new FailedValidationException("Latitude must be specified for radius searching");

                    if (lng == 0)
                        throw new FailedValidationException("Longitude must be specified for radius searching");

                    // start this clause with "and" since the companyfilter MUST be specified above 
                    parameters.Filter += $" and geo.distance(Location, geography'POINT({lng} {lat})') le {radiusKm}";
                }
                else if (searchDto.Radius == 0 && lat != 0 && lng != 0)
                {
                    // In the case of searching for a single city with no radius, set the default radius to 1 mile since most larger
                    // cities have more than one postal records, each of which contains varying lat/lng data
                    radiusKm = 1 * 1.60934; ;
                    parameters.Filter += $" and geo.distance(Location, geography'POINT({lng} {lat})') le {radiusKm}";

                }

                //facets
                if (searchDto.IsResumeUploaded.HasValue)
                    parameters.Filter += $" and IsResumeUploaded eq " + (searchDto.IsResumeUploaded.Value ? "true" : "false");

                if (searchDto.HasVideoInterview.HasValue) {
                    var filterVal = searchDto.HasVideoInterview.Value ? "ne null" : "eq null";
                    parameters.Filter += $" and VideoUrl {filterVal}";
                }

                if (searchDto.SalaryUb.HasValue)
                    parameters.Filter += $" and DesiredRate le {searchDto.SalaryUb.Value}";

                if(searchDto.SalaryLb.HasValue)
                    parameters.Filter += $" and DesiredRate ge {searchDto.SalaryLb.Value}";

                //search skills
                if (searchDto.Skill != null && searchDto.Skill.Count > 0)
                {
                    var skillsFilterExpression = searchDto.Skill.Select(s => $"skill eq '{s}'").ToList();
                    var skillsFilterString = string.Join(" or ", skillsFilterExpression);
                    parameters.Filter += $" and Skills/any(skill: {skillsFilterString})";
                }

                //search WorkPreference
                if (searchDto.WorkPreference != null && searchDto.WorkPreference.Count > 0)
                {
                    var workPreferenceFilterExpression = searchDto.Skill.Select(s => $"workpreference eq '{s}'").ToList();
                    var workPreferenceFilterString = string.Join(" or ", workPreferenceFilterExpression);
                    parameters.Filter += $" and WorkPreferences/any(workpreference: {workPreferenceFilterString})";
                }

                //search Role from Title index property
                if (searchDto.Role != null && searchDto.Role.Count > 0)
                {
                    var roleFilterExpression = searchDto.Skill.Select(s => $"Title eq '{s}'").ToList();
                    var roleFilterString = string.Join(" or ", roleFilterExpression);
                    parameters.Filter += $" and ({roleFilterString})";
                }

                //search Personality
                if (searchDto.Personality != null && searchDto.Personality.Count > 0)
                {
                    var personalitiesFilterExpression = searchDto.Skill.Select(s => $"personality eq '{s}'").ToList();
                    var personalitiesFilterString = string.Join(" or ", personalitiesFilterExpression);
                    parameters.Filter += $" and Personalities/any(personality: {personalitiesFilterString})";
                }

                //search index property Training is this same as Certification
                if (searchDto.Certification != null && searchDto.Certification.Count > 0)
                {
                    var certificationFilterExpression = searchDto.Skill.Select(s => $"trainingname/Name eq '{s}'").ToList();
                    var certificationFilterString = string.Join(" or ", certificationFilterExpression);
                    parameters.Filter += $" and Training/any(trainingname: {certificationFilterString})";
                }



                _logger.LogInformation($"HiringManagerService:_PerformAzureSearch: filter = {parameters.Filter} ");

                // double quote email to ensure direct hit         
                var keyword = Utils.EscapeQuoteEmailsInString(searchDto.Keyword);
                _logger.LogInformation($"HiringManagerService:_PerformAzureSearch: escaped keyword = {keyword} ");

                results = indexClient.Documents.Search<CandidateSDOC>(keyword, parameters);

                //Map results
                DateTime startMap = DateTime.Now;
                var G2s = results?.Results?
                    .Select(s => (CandidateSDOC)s.Document)
                    .ToList();

                searchResults.TotalHits = results.Count.Value;
                searchResults.PageSize = searchDto.Limit;
                searchResults.NumPages = searchResults.PageSize != 0 ? (int)Math.Ceiling((double)searchResults.TotalHits / searchResults.PageSize) : 0;
                //searchResults.SubscriberCount = searchResults.G2s.Count;
                searchResults.PageNum = (searchDto.Offset / searchDto.Limit) + 1;

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
            catch (Exception ex)
            {
                _logger.LogError($"HiringManagerService:_PerformAzureSearch: error = {ex.ToString()}");
                throw ex;
            }
        }

    }
}
