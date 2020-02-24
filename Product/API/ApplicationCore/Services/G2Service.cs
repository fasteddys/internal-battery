using AutoMapper;
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

        public G2Service(
            UpDiddyDbContext context,
            IConfiguration configuration,            
            IRepositoryWrapper repository,
            ILogger<SubscriberService> logger,
            IMapper mapper,
            IAzureSearchService azureSearchService
            )
        {
            _db = context;
            _configuration = configuration;            
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _azureSearchService = azureSearchService;
        }
        
        public async Task<G2SearchResultDto> SearchG2Async(int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*", int radius = 0, double lat = 0, double lng = 0)
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

            // todo jab fiture out out to deal with geo specifications 
            if ( radius > 0 )
            {

                double radiusKm = radius * 1.60934;
                if (lat == 0)
                    throw new FailedValidationException("Lattitude must be specified for radius searching");

                if (lng == 0)
                    throw new FailedValidationException("Longitude must be specified for radius searching");
               
                parameters.Filter = $"geo.distance(Location, geography'POINT({lng} {lat})') le {radiusKm}";
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


        public async Task<bool> CreateG2Async(G2Dto g2)
        {


            // todo jab map 
            G2 theG2 = new G2()
            {
                City = g2.City,
                Id = g2.Id,
                Lat = g2.Lat,
                Lng = g2.Lng
            };


            _azureSearchService.AddOrUpdateG2(theG2);

            return true;
        }



    }
}
