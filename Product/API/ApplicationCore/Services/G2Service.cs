using AutoMapper;
using GeoJSON.Net.Geometry;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace UpDiddyApi.ApplicationCore.Services
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



        // todo jab background job for creating azure record for for every  subscriber for bootstrapping Task1750
        // todo jab figure out how to update G2 when self-curated or public data changes 


        #region G2 Searching 

        public async Task<G2SearchResultDto> SearchG2Async(Guid subscriberGuid, int cityId,   int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*", int radius = 0 )
        {

            // validate the the user provides a city if they also provided a radius
            if (cityId == 0 && radius != 0)
                throw new FailedValidationException("A cityId must be provided if a radius is specifed");

            Recruiter recruiter = await _repository.RecruiterRepository.GetRecruiterAndCompanyBySubscriberGuid(subscriberGuid);

            // validate that the subscriber is a recruiter 
            if (recruiter == null)
                throw new NotFoundException($"Subscriber {subscriberGuid} is not a recruiter");
            // validate that the recruiter is asscociated with a company
            if (recruiter.CompanyId == null)
                throw new FailedValidationException($"Recruiter {recruiter.RecruiterGuid} is not associated with a company");

            // get company id for the recruiter 
            int companyId = recruiter.CompanyId.Value;
            // handle case of non geo search 
            if ( cityId == 0 )
                return await SearchG2Async(recruiter.Company.CompanyGuid, limit, offset, sort, order, keyword, 0, 0, 0 );

            // pick a random postal code for the city to get the last and long 
            Postal postal = _repository.PostalRepository.GetAll()
                .Where(p => p.CityId == cityId && p.IsDeleted == 0)
                .FirstOrDefault();

            // validate that the city has a postal code 
            if (postal == null)
                throw new NotFoundException($"A city with an Id of {cityId} cannot be found.");

            return await SearchG2Async(recruiter.Company.CompanyGuid, limit, offset, sort, order, keyword, radius, (double)postal.Latitude, (double)postal.Longitude);
        }




  



        #endregion


        #region G2 Azure Indexing Operations By Subscriber 


        /// <summary>
        /// For the given subscriber, update or add their profile to the G2 azure index 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        public async Task<bool> IndexSubscriber(Guid subscriberGuid)
        {
            // Get the public company guid for 
            Guid publicDataCompanyGuid = Guid.Parse(_configuration["CareerCircle:PublicDataCompanyGuid"]);
 
             // Get all non-public G2s for subscriber 
             List<v_ProfileAzureSearch> g2Profiles = _db.ProfileAzureSearch
             .Where( p => p.SubscriberGuid == subscriberGuid && p.CompanyGuid != publicDataCompanyGuid )
             .ToList();

             foreach (v_ProfileAzureSearch g2 in g2Profiles )
             {                   
                 if ( g2.CompanyGuid != null  )
                 {
                    G2SDOC indexDoc = await MapToG2SDOC(g2);   
                     // fire off as background job 
                    _hangfireService.Enqueue<ScheduledJobs>(j => j.G2IndexAddOrUpdate(indexDoc));                     
                 }
             };
            
            return true;
        }


        /// <summary>
        /// For the given subscriber, remove all instances of their profiles from the G2 azure index 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        public async Task<bool> RemoveSubscriberFromIndex(Guid subscriberGuid)
        {


            G2SearchResultDto subscriberDocs = await SearchG2ForSubscriberAsync(subscriberGuid);

            foreach ( G2InfoDto g2  in subscriberDocs.G2s)
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
            if ( subscriberDocs.TotalHits > subscriberDocs.SubscriberCount)
                _hangfireService.Schedule<ScheduledJobs>(j => j.G2DeleteSubscriber(subscriberGuid), TimeSpan.FromMinutes(DeleteSubscriberG2RecurseDelayInMinutes) );
 

            return true;
        }


        /// <summary>
        /// Index the specified g2 document into azure search 
        /// </summary>
        /// <param name="g2"></param>
        /// <returns></returns>
        public async Task<bool> IndexG2Async(G2SDOC g2)
        {
            _azureSearchService.AddOrUpdateG2(g2);
            return true;
        }

        /// <summary>
        /// For the given company, remove all instances of their profiles from the G2 index 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        public async Task<bool> RemoveCompanyFromIndex(Guid companyGuid)
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
                // Call background job to delete the user 
               // _hangfireService.Enqueue<ScheduledJobs>(j => j.G2IndexDelete(delDoc));
            };
            _hangfireService.Enqueue<ScheduledJobs>(j => j.G2IndexDeleteBulk(Docs));

            // If for some reason the total number of records for the given subscriber is more the number 
            // retreived, recursivley call this routine again 

            //TODO JAB Get another variable for recurse delay 

            int DeleteSubscriberG2RecurseDelayInMinutes = int.Parse(_configuration["AzureSearch:DeleteSubscriberG2RecurseDelayInMinutes"]);
            if (companyDocs.TotalHits > companyDocs.SubscriberCount)
                _hangfireService.Schedule<ScheduledJobs>(j => j.G2DeleteCompany(companyGuid), TimeSpan.FromMinutes(DeleteSubscriberG2RecurseDelayInMinutes));


            return true;
        }




        #endregion


        #region G2 Azure Indexing Operations By Company

        /// <summary>
        /// For the given subscriber, update or add their profile to the G2 azure index 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        public async Task<bool> IndexCompany(Guid companyGuid)
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
                        // fire off as background job 
                     //   _hangfireService.Enqueue<ScheduledJobs>(j => j.G2IndexAddOrUpdate(indexDoc));
                    }
                    
                };
                _hangfireService.Enqueue<ScheduledJobs>(j => j.G2IndexAddOrUpdateBulk(Docs));


                return true;
            }
            catch( Exception ex )
            {
                _logger.LogError($"G2Service:IndexCompany Error: {ex.Message} ");
                return false;
            }
       
        }


        #endregion




        #region G2 Backing Store Operations 

        /// <summary>
        /// Add a new subscriber G2 Profile record for every company to which they do not already have
        /// a g2 profile.
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns> The number of new g2 profile records added </returns>
        public async Task<int> AddSubscriberProfiles(Guid subscriberGuid)
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
        public async Task<int> DeleteSubscriberProfiles(Guid subscriberGuid)
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
        public async Task<int> AddCompanyProfiles(Guid companyGuid)
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
        public async Task<int> DeleteCompanyProfiles(Guid comanyGuid)
        {
            // call stored procedure to create a g2 recordsfor the specified subscriber.  1 record per 
            // active company will be created and return the number of records created 
            int rVal = await _repository.StoredProcedureRepository.DeleteCompanyG2Profiles(comanyGuid);

            return rVal;
        }



        #endregion


        #region   G2 Operations (backing store and indexing)

        /// <summary>
        /// Adds a new subsriber by creating a g2.profile record for every active company for the subscriber.
        /// Also indexes the subscriber into azure search
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        public async Task<bool> AddSubscriber(Guid subscriberGuid)
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
        public async Task<bool> DeleteSubscriber(Guid subscriberGuid)
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
        public async Task<bool> AddCompany(Guid companyGuid)
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
        public async Task<bool> DeleteCompany(Guid compmapnyGuid)
        {

           _hangfireService.Enqueue<ScheduledJobs>(j => j.G2DeleteCompany(compmapnyGuid));
            return true;
        }


        #endregion


        #region private helper functions 


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

            // Create an index named hotels
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

            // Create an index named hotels
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
            searchResults.PageSize =  (int) results.Count.Value;
            searchResults.NumPages = searchResults.PageSize != 0 ? (int)Math.Ceiling((double)searchResults.TotalHits / searchResults.PageSize) : 0;
            searchResults.SubscriberCount = searchResults.G2s.Count;
            searchResults.PageNum =  1;

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
                if ( g2.Location != null )
                {
                    Double lat = (double)g2.Location.Lat;
                    Double lng = (double)g2.Location.Long;
                    Position p = new Position(lat, lng);
                    indexDoc.Location = new Point(p);

                }
                // manually map the location.  todo find a way for automapper to do this 
                return indexDoc;
            }
            catch ( Exception ex )
            {
                _logger.LogError($"G2Service:MapToG2SDOC Exception for {g2.ProfileGuid} error = {ex.Message}");
                throw ex;
            }
        }
 
        private async Task<G2SearchResultDto> SearchG2Async(Guid companyGuid, int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*", int radius = 0, double lat = 0, double lng = 0)
        {
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

            // Create an index named hotels
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


            // IMPORTANT!!!!   Filter quereies to be withing the specified company id for security reasons 
            parameters.Filter = $"CompanyGuid eq '{companyGuid}'";

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
            else if ( radius == 0 && lat != 0 && lng != 0)
            {
                // In the case of searching for a single city with no radius, set the default radius to 1 mile since most larger
                // cities have more than one postal records, each of which contains varying lat/lng data
                radiusKm = 1 * 1.60934; ;
                parameters.Filter += $" and geo.distance(Location, geography'POINT({lng} {lat})') le {radiusKm}";
 
            }
            
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

        #endregion


    }
}
