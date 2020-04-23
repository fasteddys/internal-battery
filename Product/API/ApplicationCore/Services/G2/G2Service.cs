using AutoMapper;
using GeoJSON.Net.Geometry;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Models.Views;
using UpDiddyApi.Workflow;
using UpDiddyLib.Domain.AzureSearchDocuments;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Services.G2
{
    public class G2Service : IG2Service
    {
        private readonly UpDiddyDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IRepositoryWrapper _repository;
        private readonly IMapper _mapper;
        private readonly IAzureSearchService _azureSearchService;
        private readonly IHangfireService _hangfireService;
        private readonly int MaxAzureSearchQueryResults = 1000;

        public G2Service(
            UpDiddyDbContext context,
            IConfiguration configuration,
            IRepositoryWrapper repository,
            ILogger<SubscriberService> logger,
            IMapper mapper,
            IAzureSearchService azureSearchService,
            IHangfireService hangfireService
            )
        {
            _db = context;
            _configuration = configuration;
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _azureSearchService = azureSearchService;
            _hangfireService = hangfireService;
        }

        #region G2 Searching 

        public async Task<G2SearchResultDto> G2SearchAsync(Guid subscriberGuid, Guid cityGuid, int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*", Guid? sourcePartnerGuid = null, int radius = 0, bool? isWillingToRelocate = null, bool? isWillingToTravel = null, bool? isActiveJobSeeker = null, bool? isCurrentlyEmployed = null, bool? isWillingToWorkProBono = null)
        {

            // validate the the user provides a city if they also provided a radius
            if ((cityGuid == null || cityGuid == Guid.Empty) && radius != 0)
                throw new FailedValidationException("A city guid must be provided if a radius is specifed");

            Recruiter recruiter = await _repository.RecruiterRepository.GetRecruiterAndCompanyBySubscriberGuid(subscriberGuid);

            // validate that the subscriber is a recruiter 
            if (recruiter == null)
                throw new NotFoundException($"Subscriber {subscriberGuid} is not a recruiter");
            // validate that the recruiter is asscociated with a company
            if (recruiter.RecruiterCompanies == null || recruiter.RecruiterCompanies.FirstOrDefault() == null)
                throw new FailedValidationException($"Recruiter {recruiter.RecruiterGuid} is not associated with a company");

            // limit search to only the careercircle company for now....  
            string CCCompanySearchGuid = _configuration["AzureSearch:CCCompanySearchGuid"];

            List<Guid> companyGuids = recruiter.RecruiterCompanies
                .Where(g => g.IsDeleted == 0 & g.Company.CompanyGuid == Guid.Parse(CCCompanySearchGuid))
                .Select(g => g.Company.CompanyGuid)
                .ToList();


            if (companyGuids == null || companyGuids.FirstOrDefault() == null)
                throw new FailedValidationException($"Recruiter {recruiter.RecruiterGuid} is not associated with the default carreer circle search company");

            // handle case of non geo search 
            if (cityGuid == null || cityGuid == Guid.Empty)
                return await SearchG2Async(companyGuids, limit, offset, sort, order, keyword, sourcePartnerGuid, 0, 0, 0, isWillingToRelocate, isWillingToTravel, isActiveJobSeeker, isCurrentlyEmployed, isWillingToWorkProBono);

            // pick a random postal code for the city to get the last and long 
            Postal postal = _repository.PostalRepository.GetAll()
                .Include(c => c.City)
                .Where(p => p.City.CityGuid == cityGuid && p.IsDeleted == 0)
                .FirstOrDefault();

            // validate that the city has a postal code 
            if (postal == null)
                throw new NotFoundException($"A city with an Guid of {cityGuid} cannot be found.");

            return await SearchG2Async(companyGuids, limit, offset, sort, order, keyword, sourcePartnerGuid, radius, (double)postal.Latitude, (double)postal.Longitude, isWillingToRelocate, isWillingToTravel, isActiveJobSeeker, isCurrentlyEmployed, isWillingToWorkProBono);
        }



        public async Task<G2SearchResultDto> G2SearchGetTopAsync(int numRecords)
        {
            DateTime startSearch = DateTime.Now;
            G2SearchResultDto searchResults = new G2SearchResultDto();

            string searchServiceName = _configuration["AzureSearch:SearchServiceName"];
            string adminApiKey = _configuration["AzureSearch:SearchServiceQueryApiKey"];
            string g2IndexName = _configuration["AzureSearch:G2IndexName"];



            SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));
            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient(g2IndexName);

            SearchParameters parameters;
            DocumentSearchResult<G2InfoDto> results;

            parameters =
                new SearchParameters()
                {
                    IncludeTotalResultCount = true,
                    Top = numRecords
                };


            results = indexClient.Documents.Search<G2InfoDto>("*", parameters);

            DateTime startMap = DateTime.Now;
            searchResults.G2s = results?.Results?
                .Select(s => (G2InfoDto)s.Document)
                .ToList();

            searchResults.TotalHits = results.Count.Value;
            searchResults.PageSize = (int)results.Count.Value;
            searchResults.NumPages = searchResults.PageSize != 0 ? (int)Math.Ceiling((double)searchResults.TotalHits / searchResults.PageSize) : 0;
            searchResults.SubscriberCount = searchResults.G2s.Count;
            searchResults.PageNum = 1;

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


        #endregion

        #region G2 Azure Indexing 


        public async Task<bool> G2IndexBulkDeleteByGuidAsync(List<Guid> guidList)
        {
            List<G2SDOC> Docs = new List<G2SDOC>();
            foreach (Guid g in guidList)
            {
                G2SDOC delDoc = new G2SDOC()
                {
                    ProfileGuid = g
                };
                Docs.Add(delDoc);
            };

            await G2IndexDeleteBulkAsync(Docs);
            return true;
        }

        public async Task<bool> G2IndexBySubscriberAsync(Guid subscriberGuid, Guid companyGuid)
        {
            // Get the public company guid for 
            Guid publicDataCompanyGuid = Guid.Parse(_configuration["CareerCircle:PublicDataCompanyGuid"]);

            // Get all non-public G2s for subscriber 
            List<v_ProfileAzureSearch> g2Profiles = _db.ProfileAzureSearch
            .Where(p => p.SubscriberGuid == subscriberGuid && p.CompanyGuid != publicDataCompanyGuid && p.CompanyGuid == companyGuid)
            .ToList();

            if (g2Profiles.Count == 0)
                throw new NotFoundException($"G2Service:G2IndexBySubscriberAsync: Could not find g2 for subscriber {subscriberGuid} for company {companyGuid}");

            if (g2Profiles.Count > 1)
                throw new FailedValidationException($"G2Service:G2IndexBySubscriberAsync:  SUubscriber {subscriberGuid} has {g2Profiles.Count} for  company {companyGuid}.  Only 1 profile is allowed per company.");

            v_ProfileAzureSearch g2 = g2Profiles[0];
            if (g2.CompanyGuid != null)
            {
                G2SDOC indexDoc = await MapToG2SDOC(g2);
                // fire off as background job 
                _hangfireService.Enqueue<ScheduledJobs>(j => j.G2IndexAddOrUpdate(indexDoc));
            }
            return true;
        }



        /// <summary>
        /// For the given subscriber, update or add their profile to the G2 azure index 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        public async Task<bool> G2IndexBySubscriberAsync(Guid subscriberGuid)
        {
            // Get the public company guid for 
            Guid publicDataCompanyGuid = Guid.Parse(_configuration["CareerCircle:PublicDataCompanyGuid"]);

            // Get all non-public G2s for subscriber 
            List<v_ProfileAzureSearch> g2Profiles = _db.ProfileAzureSearch
            .Where(p => p.SubscriberGuid == subscriberGuid && p.CompanyGuid != publicDataCompanyGuid)
            .ToList();

            List<G2SDOC> Docs = new List<G2SDOC>();
            foreach (v_ProfileAzureSearch g2 in g2Profiles)
            {
                if (g2.CompanyGuid != null)
                {
                    G2SDOC indexDoc = await MapToG2SDOC(g2);
                    Docs.Add(indexDoc);
                }
            };
            // fire off as background job 
            _hangfireService.Enqueue<ScheduledJobs>(j => j.G2IndexAddOrUpdateBulk(Docs));

            return true;
        }


        /// <summary>
        /// For the given subscriber, remove all instances of their profiles from the G2 azure index 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        public async Task<bool> G2IndexRemoveSubscriberAsync(Guid subscriberGuid)
        {
            G2SearchResultDto subscriberDocs = await SearchG2ForSubscriberAsync(subscriberGuid);

            foreach (G2InfoDto g2 in subscriberDocs.G2s)
            {
                G2SDOC delDoc = new G2SDOC()
                {
                    ProfileGuid = g2.ProfileGuid,
                };
                // Call background job to delete the user 
                _hangfireService.Enqueue<ScheduledJobs>(j => j.G2IndexDelete(delDoc));
            };

            // If for some reason the total number of records for the given subscriber is more the number 
            // retreived, recursivley call this routine again 
            int DeleteSubscriberG2RecurseDelayInMinutes = int.Parse(_configuration["AzureSearch:DeleteSubscriberG2RecurseDelayInMinutes"]);
            if (subscriberDocs.TotalHits > subscriberDocs.SubscriberCount)
                _hangfireService.Schedule<ScheduledJobs>(j => j.G2DeleteSubscriber(subscriberGuid), TimeSpan.FromMinutes(DeleteSubscriberG2RecurseDelayInMinutes));

            return true;
        }


        /// <summary>
        /// Index the specified g2 document into azure search 
        /// </summary>
        /// <param name="g2"></param>
        /// <returns></returns>
        public async Task<bool> G2IndexAsync(G2SDOC g2)
        {
            AzureIndexResult info = await _azureSearchService.AddOrUpdateG2(g2);
            await UpdateG2Status(info, Constants.G2AzureIndexStatus.Indexed, info.StatusMsg);
            return true;
        }

        /// <summary>
        /// For the given company, remove all instances of their profiles from the G2 index 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        public async Task<bool> G2IndexRemoveCompanyProflesAsync(Guid companyGuid)
        {
            G2SearchResultDto companyDocs = await SearchG2ForCompanyAsync(companyGuid);

            List<G2SDOC> Docs = new List<G2SDOC>();
            foreach (G2InfoDto g2 in companyDocs.G2s)
            {
                G2SDOC delDoc = new G2SDOC()
                {
                    ProfileGuid = g2.ProfileGuid,
                };
                Docs.Add(delDoc);
            };
            // Call background job to delete the user 
            _hangfireService.Enqueue<ScheduledJobs>(j => j.G2IndexDeleteBulk(Docs));

            // If for some reason the total number of records for the given subscriber is more the number 
            // retreived, recursivley call this routine again 
            int DeleteSubscriberG2RecurseDelayInMinutes = int.Parse(_configuration["AzureSearch:DeleteCompanyG2RecurseDelayInMinutes"]);
            if (companyDocs.TotalHits > companyDocs.SubscriberCount)
                _hangfireService.Schedule<ScheduledJobs>(j => j.G2DeleteCompany(companyGuid), TimeSpan.FromMinutes(DeleteSubscriberG2RecurseDelayInMinutes));


            return true;
        }



        public async Task<bool> G2IndexAddOrUpdateAsync(G2SDOC g2)
        {
            _logger.Log(LogLevel.Information, $"G2Service.G2IndexAddOrUpdateAsync starting index for g2 {g2.ProfileGuid}");
            AzureIndexResult info = await _azureSearchService.AddOrUpdateG2(g2);
            await UpdateG2Status(info, Constants.G2AzureIndexStatus.Indexed, info.StatusMsg);
            _logger.Log(LogLevel.Information, $"G2Service.G2IndexAddOrUpdateAsync done index for g2 {g2.ProfileGuid}");
            return true;
        }



        /// <summary>
        /// For the given coompany, update or add their g2 profiles to the azure index 
        /// </summary>
        /// <param name="companyGuid"></param>
        /// <returns></returns>
        public async Task<bool> G2IndexCompanyProflesAsync(Guid companyGuid)
        {
            try
            {
                // Get the public company guid for 
                Guid publicDataCompanyGuid = Guid.Parse(_configuration["CareerCircle:PublicDataCompanyGuid"]);

                // Get all non-public G2s for subscriber 
                List<v_ProfileAzureSearch> g2Profiles = _db.ProfileAzureSearch
                .Where(p => p.CompanyGuid == companyGuid && p.CompanyGuid != publicDataCompanyGuid)
                .ToList();

                List<G2SDOC> Docs = new List<G2SDOC>();

                int counter = 0;
                foreach (v_ProfileAzureSearch g2 in g2Profiles)
                {
                    if (g2.CompanyGuid != null)
                    {
                        ++counter;
                        G2SDOC indexDoc = await MapToG2SDOC(g2);
                        Docs.Add(indexDoc);

                    }

                };
                _hangfireService.Enqueue<ScheduledJobs>(j => j.G2IndexAddOrUpdateBulk(Docs));


                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"G2Service:G2IndexCompanyProflesAsync Error: {ex.Message} ");
                return false;
            }

        }


        public async Task<bool> G2IndexProfileByGuidAsync(Guid g2ProfileGuid)
        {

            // Get all non-public G2s for subscriber 
            List<v_ProfileAzureSearch> g2Profiles = _db.ProfileAzureSearch
            .Where(p => p.ProfileGuid == g2ProfileGuid)
            .ToList();

            if (g2Profiles.Count == 0)
                throw new NotFoundException($"G2Service:G2IndexProfileByGuidAsync: Could not find g2 for subscriber {g2ProfileGuid} ");

            if (g2Profiles.Count > 1)
                throw new FailedValidationException($"G2Service:G2IndexProfileByGuidAsync:  SUubscriber {g2ProfileGuid}");

            v_ProfileAzureSearch g2 = g2Profiles[0];
            if (g2.CompanyGuid != null)
            {
                G2SDOC indexDoc = await MapToG2SDOC(g2);
                // fire off as background job 
                _hangfireService.Enqueue<ScheduledJobs>(j => j.G2IndexAddOrUpdate(indexDoc));
            }


            return true;
        }


        /// <summary>
        /// Index a batch of g2 profiles 
        /// </summary>
        /// <param name="g2Profiles"></param>
        /// <returns></returns>
        public async Task<bool> G2IndexBulkByProfileAzureSearchAsync(List<v_ProfileAzureSearch> g2Profiles)
        {
            if (g2Profiles == null || g2Profiles.Count == 0)
            {
                _logger.LogInformation($"G2Service:G2IndexBulkByProfileAzureSearchAsync There are no profiles to index in the current batch, returing false");
                return false;
            }

            _logger.LogInformation($"G2Service:G2IndexBulkByProfileAzureSearchAsync Starting with a batch of  {g2Profiles.Count} for indexing");
            List<G2SDOC> Docs = new List<G2SDOC>();
            foreach (v_ProfileAzureSearch g2 in g2Profiles)
            {
                if (g2.CompanyGuid != null)
                {
                    G2SDOC indexDoc = await MapToG2SDOC(g2);
                    Docs.Add(indexDoc);
                }
            };

            // Don't queue the hangfire job if there is nothing to do
            if (Docs.Count == 0)
                return true;

            _hangfireService.Enqueue<ScheduledJobs>(j => j.G2IndexAddOrUpdateBulk(Docs));
            _logger.LogInformation($"G2Service:G2IndexBulkByProfileAzureSearchAsync Done");

            return true;


        }

        /// <summary>
        /// Bulk index into azure 
        /// </summary>
        /// <param name="g2List"></param>
        /// <returns></returns>
        public async Task<bool> G2IndexBulkAsync(List<G2SDOC> g2List)
        {
            // If there is no work to do jus return true
            if (g2List.Count == 0)
                return true;
            _logger.Log(LogLevel.Information, $"G2Service.G2IndexBulkAsync starting index for g2");
            AzureIndexResult info = await _azureSearchService.AddOrUpdateG2Bulk(g2List);
            await UpdateG2Status(info, ResolveIndexStatusMessage(info.StatusMsg), info.StatusMsg);
            _logger.Log(LogLevel.Information, $"G2Service.G2IndexBulkAsync done index for g2");
            return true;
        }



        /// <summary>
        /// Bulk delete of items from azure index - delete means delete the info from azure index and mark the item in
        /// the backing store as deleted
        /// </summary>
        /// <param name="g2"></param>
        /// <returns></returns>
        public async Task<bool> G2IndexDeleteBulkAsync(List<G2SDOC> g2List)
        {
            _logger.Log(LogLevel.Information, $"G2Service.G2IndexDeleteBulkAsync starting index delete");
            AzureIndexResult info = await _azureSearchService.DeleteG2Bulk(g2List);
            await UpdateG2Status(info, Constants.G2AzureIndexStatus.Deleted, info.StatusMsg);
            _logger.Log(LogLevel.Information, $"G2Service.G2IndexDeleteBulkAsync done index delete");
            return true;
        }



        /// <summary>
        /// Bulk purge of items from azure index - purge means delete the info from azure index and mark the item in
        /// the backing store as unidexed 
        /// </summary>
        /// <param name="g2"></param>
        /// <returns></returns>
        public async Task<bool> G2IndexPurgeBulkAsync(List<G2SDOC> g2List)
        {
            _logger.Log(LogLevel.Information, $"G2Service.G2IndexPurgeBulkAsync starting index delete");
            AzureIndexResult info = await _azureSearchService.DeleteG2Bulk(g2List);
            // set index status to "none" for purges so these items will be reindex the next time a global re-index of 
            // all un-indexed items is done 
            await UpdateG2Status(info, Constants.G2AzureIndexStatus.None, info.StatusMsg);
            _logger.Log(LogLevel.Information, $"G2Service.G2IndexPurgeBulkAsync done index delete");
            return true;
        }


        /// <summary>
        /// Add or update the G2 into the azure search index 
        /// </summary>
        /// <param name="g2"></param>
        /// <returns></returns>
        public async Task<bool> G2IndexDeleteAsync(G2SDOC g2)
        {
            _logger.Log(LogLevel.Information, $"G2Service.G2IndexDeleteAsync starting index for g2 {g2.ProfileGuid}");
            AzureIndexResult info = await _azureSearchService.DeleteG2(g2);
            await UpdateG2Status(info, Constants.G2AzureIndexStatus.Deleted, info.StatusMsg);
            _logger.Log(LogLevel.Information, $"G2Service.G2IndexDeleteAsync done index for g2 {g2.ProfileGuid}");
            return true;
        }


        #endregion

        #region G2 Profile Operations 

        /// <summary>
        /// Add a new subscriber G2 Profile record for every company to which they do not already have
        /// a g2 profile.
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns> The number of new g2 profile records added </returns>
        public async Task<int> G2ProfileAddSubscriberAsync(Guid subscriberGuid)
        {
            // call stored procedure to create a g2 recordsfor the specified subscriber.  1 record per 
            // active company will be created and return the number of records created 
            int rVal = await _repository.StoredProcedureRepository.CreateSubscriberG2Profiles(subscriberGuid);

            return rVal;
        }

        /// <summary>
        /// Remove subscriber profile records 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        public async Task<int> G2ProfileDeleteSubscriberAsync(Guid subscriberGuid)
        {
            // call stored procedure to create a g2 recordsfor the specified subscriber.  1 record per 
            // active company will be created and return the number of records created 
            int rVal = await _repository.StoredProcedureRepository.DeleteSubscriberG2Profiles(subscriberGuid);

            return rVal;
        }


        /// <summary>
        /// Adds a new subscriber G2 profile record for the specified company for every active subscriber 
        /// </summary>
        /// <param name="companyGuid"></param>
        /// <returns></returns>
        public async Task<int> G2ProfileAddByCompany(Guid companyGuid)
        {
            // call stored procedure to create a g2 recordsfor the specified subscriber.  1 record per 
            // active company will be created and return the number of records created 
            int rVal = await _repository.StoredProcedureRepository.CreateCompanyG2Profiles(companyGuid);

            return rVal;
        }



        /// <summary>
        /// Remove company profile records 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        public async Task<int> G2ProfileDeleteByCompany(Guid comanyGuid)
        {
            // call stored procedure to create a g2 recordsfor the specified subscriber.  1 record per 
            // active company will be created and return the number of records created 
            int rVal = await _repository.StoredProcedureRepository.DeleteCompanyG2Profiles(comanyGuid);

            return rVal;
        }



        #endregion

        #region  G2 Operations

        /// <summary>
        /// Adds a new subscriber by creating a g2.profile record for every active company for the subscriber.
        /// Also indexes the subscriber into azure search
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        public async Task<bool> G2AddSubscriberAsync(Guid subscriberGuid)
        {
            _hangfireService.Enqueue<ScheduledJobs>(j => j.G2AddNewSubscriber(subscriberGuid));
            return true;
        }

        /// <summary>
        /// Removes  all of the subscribers g2.profile information in sql/server  and removes them from the azure search
        /// // index
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        public async Task<bool> G2DeleteSubscriberAsync(Guid subscriberGuid)
        {
            _hangfireService.Enqueue<ScheduledJobs>(j => j.G2DeleteSubscriber(subscriberGuid));
            return true;
        }


        /// <summary>
        /// Adds a new company by creating a g2.profile record for every active subscriber for the given company.
        /// Also indexes the subscriber into azure search
        /// </summary>
        /// <param name="companyGuid"></param>
        /// <returns></returns>
        public async Task<bool> G2AddCompanyAsync(Guid companyGuid)
        {

            _hangfireService.Enqueue<ScheduledJobs>(j => j.G2AddNewCompany(companyGuid));
            return true;
        }

        /// <summary>
        /// Removes  all of the company's g2.profile information in sql/server  and removes then from the azure search
        /// // index
        /// </summary>
        /// <param name="companyGuid"></param>
        /// <returns></returns>
        public async Task<bool> G2DeleteCompanyAsync(Guid compmapnyGuid)
        {

            _hangfireService.Enqueue<ScheduledJobs>(j => j.G2DeleteCompany(compmapnyGuid));
            return true;
        }

        /// <summary>
        /// Adds a g2 profile for active subscribers who do not have one.  Also indexed these subscribers into azure  
        /// Should only be called once when the product is launched.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> G2AddNewSubscribers()
        {
            int numNewProfiles = await _repository.StoredProcedureRepository.BootG2Profiles();
            // Kick off job to index any unindexed g2 profiles 
            _hangfireService.Enqueue<ScheduledJobs>(j => j.G2IndexUnindexedProfiles());
            return true;
        }

        /// <summary>
        /// Remove all documents from the azure index
        /// </summary>
        /// <returns></returns>
        public async Task<bool> G2IndexPurgeAsync()
        {
            // Kick off job to index any unindexed g2 profiles 
            _hangfireService.Enqueue<ScheduledJobs>(j => j.G2IndexPurge());
            return true;
        }



        #endregion


        #region private helper functions 

        private async Task<bool> UpdateG2Status(AzureIndexResult results, string statusName, string info)
        {
            // Call stored procedure 
            try
            {
                _repository.StoredProcedureRepository.UpdateG2AzureIndexStatuses(results?.DOCResults?.Value ?? new List<AzureIndexResultStatus>(), statusName, info);
            }
            catch (Exception ex)
            {
                _logger.LogError($"G2Service:UpdateG2Status Error updating index statuses; message: {ex.Message}, stack trace: {ex.StackTrace}");
                throw;
            }
            return true;
        }


        /// <summary>
        ///  return all records for the specified subscriber 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        private async Task<G2SearchResultDto> SearchG2ForCompanyAsync(Guid companyGuid)
        {
            DateTime startSearch = DateTime.Now;
            G2SearchResultDto searchResults = new G2SearchResultDto();

            string searchServiceName = _configuration["AzureSearch:SearchServiceName"];
            string adminApiKey = _configuration["AzureSearch:SearchServiceQueryApiKey"];
            string g2IndexName = _configuration["AzureSearch:G2IndexName"];



            SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));
            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient(g2IndexName);

            SearchParameters parameters;
            DocumentSearchResult<G2InfoDto> results;

            parameters =
                new SearchParameters()
                {
                    IncludeTotalResultCount = true,
                    Top = MaxAzureSearchQueryResults
                };

            parameters.Filter = $"CompanyGuid eq '{companyGuid}'";

            results = indexClient.Documents.Search<G2InfoDto>("*", parameters);

            DateTime startMap = DateTime.Now;
            searchResults.G2s = results?.Results?
                .Select(s => (G2InfoDto)s.Document)
                .ToList();

            searchResults.TotalHits = results.Count.Value;
            searchResults.PageSize = (int)results.Count.Value;
            searchResults.NumPages = searchResults.PageSize != 0 ? (int)Math.Ceiling((double)searchResults.TotalHits / searchResults.PageSize) : 0;
            searchResults.SubscriberCount = searchResults.G2s.Count;
            searchResults.PageNum = 1;

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










        /// <summary>
        ///  return all records for the specified subscriber 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        private async Task<G2SearchResultDto> SearchG2ForSubscriberAsync(Guid subscriberGuid)
        {
            DateTime startSearch = DateTime.Now;
            G2SearchResultDto searchResults = new G2SearchResultDto();

            string searchServiceName = _configuration["AzureSearch:SearchServiceName"];
            string adminApiKey = _configuration["AzureSearch:SearchServiceQueryApiKey"];
            string g2IndexName = _configuration["AzureSearch:G2IndexName"];



            SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));
            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient(g2IndexName);

            SearchParameters parameters;
            DocumentSearchResult<G2InfoDto> results;

            parameters =
                new SearchParameters()
                {
                    IncludeTotalResultCount = true,
                    Top = MaxAzureSearchQueryResults
                };

            parameters.Filter = $"SubscriberGuid eq '{subscriberGuid}'";

            results = indexClient.Documents.Search<G2InfoDto>("*", parameters);

            DateTime startMap = DateTime.Now;
            searchResults.G2s = results?.Results?
                .Select(s => (G2InfoDto)s.Document)
                .ToList();

            searchResults.TotalHits = results.Count.Value;
            searchResults.PageSize = (int)results.Count.Value;
            searchResults.NumPages = searchResults.PageSize != 0 ? (int)Math.Ceiling((double)searchResults.TotalHits / searchResults.PageSize) : 0;
            searchResults.SubscriberCount = searchResults.G2s.Count;
            searchResults.PageNum = 1;

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



        private async Task<G2SDOC> MapToG2SDOC(v_ProfileAzureSearch g2)
        {
            try
            {
                G2SDOC indexDoc = _mapper.Map<G2SDOC>(g2);
                if (g2.Location != null)
                {
                    Double lat = (double)g2.Location.Lat;
                    Double lng = (double)g2.Location.Long;
                    Position p = new Position(lat, lng);
                    indexDoc.Location = new Point(p);

                }
                // manually map the location.  todo find a way for automapper to do this 
                return indexDoc;
            }
            catch (Exception ex)
            {
                _logger.LogError($"G2Service:MapToG2SDOC Exception for profile {g2.ProfileGuid}; error: {ex.Message}, stack trace: {ex.StackTrace}");
                throw;
            }
        }


        private async Task<G2SearchResultDto> SearchG2Async(List<Guid> companyGuids, int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*", Guid? sourcePartnerGuid = null, int radius = 0, double lat = 0, double lng = 0, bool? isWillingToRelocate = null, bool? isWillingToTravel = null, bool? isActiveJobSeeker = null, bool? isCurrentlyEmployed = null, bool? isWillingToWorkProBono = null)
        {

            if (companyGuids == null || companyGuids.Count == 0)
                throw new FailedValidationException("G2Service:SearchG2Async: Recruiter is not associated with the CareerCircle search company");

            DateTime startSearch = DateTime.Now;
            G2SearchResultDto searchResults = new G2SearchResultDto();

            string searchServiceName = _configuration["AzureSearch:SearchServiceName"];
            string adminApiKey = _configuration["AzureSearch:SearchServiceQueryApiKey"];
            string g2IndexName = _configuration["AzureSearch:G2IndexName"];

            // map descending to azure search sort syntax of "asc" or "desc"  default is ascending so only map descending 
            string orderBy = sort;
            if (order == "descending")
                orderBy = orderBy + " desc";
            List<String> orderByList = new List<string>();
            orderByList.Add(orderBy);

            SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));
            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient(g2IndexName);

            SearchParameters parameters;
            DocumentSearchResult<G2InfoDto> results;

            parameters =
                new SearchParameters()
                {
                    Top = limit,
                    Skip = offset,
                    OrderBy = orderByList,
                    IncludeTotalResultCount = true,
                };


            string companyFilter = string.Empty;
            foreach (Guid g in companyGuids)
            {
                if (string.IsNullOrEmpty(companyFilter))
                    companyFilter = $"( CompanyGuid eq '{g}'";
                else
                    companyFilter += $" or CompanyGuid eq '{g}'";
            }
            companyFilter += ") ";
            parameters.Filter = companyFilter;

            // Add partner filter if one has been specified
            if (sourcePartnerGuid != null)
                parameters.Filter += $" and PartnerGuid eq '{sourcePartnerGuid}'";

            // Add filter for is.... properties 
            if (isWillingToRelocate != null)
                parameters.Filter += $" and IsWillingToRelocate eq " + (isWillingToRelocate.Value ? "true" : "false");
            if (isWillingToTravel != null)
                parameters.Filter += $" and IsWillingToTravel eq " + (isWillingToTravel.Value ? "true" : "false");
            if (isWillingToWorkProBono != null)
                parameters.Filter += $" and IsWillingToWorkProBono eq " + (isWillingToWorkProBono.Value ? "true" : "false");
            if (isCurrentlyEmployed != null)
                parameters.Filter += $" and IsCurrentlyEmployed eq " + (isCurrentlyEmployed.Value ? "true" : "false");
            if (isActiveJobSeeker != null)
                parameters.Filter += $" and IsActiveJobSeeker eq " + (isActiveJobSeeker.Value ? "true" : "false");

            double radiusKm = 0;
            // check to see if radius is in play
            if (radius > 0)
            {

                radiusKm = radius * 1.60934;
                if (lat == 0)
                    throw new FailedValidationException("Lattitude must be specified for radius searching");

                if (lng == 0)
                    throw new FailedValidationException("Longitude must be specified for radius searching");

                // start this clause with "and" since the companyfilter MUST be specified above 
                parameters.Filter += $" and geo.distance(Location, geography'POINT({lng} {lat})') le {radiusKm}";
            }
            else if (radius == 0 && lat != 0 && lng != 0)
            {
                // In the case of searching for a single city with no radius, set the default radius to 1 mile since most larger
                // cities have more than one postal records, each of which contains varying lat/lng data
                radiusKm = 1 * 1.60934; ;
                parameters.Filter += $" and geo.distance(Location, geography'POINT({lng} {lat})') le {radiusKm}";

            }
            // double quote email to ensure direct hit         
            keyword = Utils.EscapeQuoteEmailsInString(keyword);

            results = indexClient.Documents.Search<G2InfoDto>(keyword, parameters);

            DateTime startMap = DateTime.Now;
            searchResults.G2s = results?.Results?
                .Select(s => (G2InfoDto)s.Document)
                .ToList();

            searchResults.TotalHits = results.Count.Value;
            searchResults.PageSize = limit;
            searchResults.NumPages = searchResults.PageSize != 0 ? (int)Math.Ceiling((double)searchResults.TotalHits / searchResults.PageSize) : 0;
            searchResults.SubscriberCount = searchResults.G2s.Count;
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

        private static string ResolveIndexStatusMessage(string statusMsg)
        {
            if (string.IsNullOrEmpty(statusMsg)) { return Constants.G2AzureIndexStatus.None; }
            if (statusMsg.StartsWith("Indexed On")) { return Constants.G2AzureIndexStatus.Indexed; }
            if (statusMsg.StartsWith("Deleted On")) { return Constants.G2AzureIndexStatus.Deleted; }
            if (statusMsg.StartsWith("StatusCode = ")) { return Constants.G2AzureIndexStatus.Error; }
            if (statusMsg.Contains("error", StringComparison.CurrentCultureIgnoreCase)) { return Constants.G2AzureIndexStatus.Error; }

            return Constants.G2AzureIndexStatus.Pending;
        }
        #endregion
    }
}
