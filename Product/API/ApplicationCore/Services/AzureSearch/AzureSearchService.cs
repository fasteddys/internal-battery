
using AutoMapper;
using GeoJSON.Net.Geometry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.AzureSearch;
using UpDiddyLib.Domain.AzureSearchDocuments;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Services.AzureSearch
{

    public class AzureSearchService : IAzureSearchService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private UpDiddyDbContext _db { get; set; }
        private IConfiguration _configuration { get; set; }
        private ICloudStorage _cloudStorage { get; set; }
        private ILogger _logger { get; set; }
        private IRepositoryWrapper _repository { get; set; }
        private readonly IMapper _mapper; 
        private IHangfireService _hangfireService { get; set; } 
        private ISysEmail _sysEmail;

        public AzureSearchService(
            IHttpClientFactory httpClientFactory,
            UpDiddyDbContext context,
            IConfiguration configuration,
            ICloudStorage cloudStorage,
            IRepositoryWrapper repository,
            ILogger<SubscriberService> logger,
            IMapper mapper,            
            IHangfireService hangfireService,            
            ISysEmail sysEmail,
            IButterCMSService butterCMSService
            )
        {           
            _httpClientFactory = httpClientFactory;
            _db = context;
            _configuration = configuration;
            _cloudStorage = cloudStorage;
            _repository = repository;
            _logger = logger;
            _mapper = mapper;            
            _hangfireService = hangfireService;            
            _sysEmail = sysEmail;            
        }



        #region G2

        // TODO JAB Implement 
        public async Task<bool> AddOrUpdateG2(G2Test g2)
        {
            SendG2Request(g2, "upload");
            return true;
        }

        public async Task<bool> DeleteG2(G2Test g2)
        {
            SendG2Request(g2, "delete");
            return true;
        }


        #endregion


        #region subscriber index 
        public async Task<bool> AddOrUpdateSubscriber(Subscriber subscriber)
        {
            SendSubscriberRequest(subscriber, "upload");            
            return true;
        }

        public async Task<bool> DeleteSubscriber(Subscriber subscriber)
        {
            SendSubscriberRequest(subscriber, "delete");            
            return true;
        }

        #endregion


        #region Recruiter index 
        public async Task<bool> AddOrUpdateRecruiter(Recruiter recruiter)
        {
            SendRecruiterRequest(recruiter, "upload");
            return true;
        }

        public async Task<bool> DeleteRecruiter(Recruiter recruiter)
        {
            SendRecruiterRequest(recruiter, "delete");
            return true;
        }

        #endregion



        #region helper functions 


        private async Task<bool> SendG2Request(G2Test g2, string cmd)
        {
            // fire and forget 
            Task.Run(() =>
            {
                string index = _configuration["AzureSearch:G2IndexName"];
                SDOCRequest<G2SDOC> docs = new SDOCRequest<G2SDOC>();
                // TODO JAB Use Mapper if possible 
                // RecruiterSDOC doc = _mapper.Map<RecruiterSDOC>(recruiter);
                G2SDOC doc = new G2SDOC()
                {
                    City = g2.City,
                    Id = g2.Id
                };

                Position position = new Position(g2.Lat, g2.Lng);      
                doc.Location = new Point(position);
                doc.SearchAction = cmd;
                docs.value.Add(doc);
                string Json = Newtonsoft.Json.JsonConvert.SerializeObject(docs);
                SendSearchRequest(index, Json);
            });
            return true;
        }


 

        private async Task<bool> SendRecruiterRequest(Recruiter recruiter, string cmd)
        {
            // fire and forget 
            Task.Run(() => {
                string index = _configuration["AzureSearch:RecruiterIndexName"];
                SDOCRequest<RecruiterSDOC> docs = new SDOCRequest<RecruiterSDOC>();
                RecruiterSDOC doc = _mapper.Map<RecruiterSDOC>(recruiter);
                doc.SearchAction = cmd;
                docs.value.Add(doc);
                string Json = Newtonsoft.Json.JsonConvert.SerializeObject(docs);
                SendSearchRequest(index, Json);
            });
            return true;
        }



        private async Task<bool> SendSubscriberRequest(Subscriber subscriber, string cmd)
        {
            // fire and forget 
            Task.Run(() => {
                string index = _configuration["AzureSearch:SubscriberIndexName"];
                SDOCRequest<SubscriberSDOC> docs = new SDOCRequest<SubscriberSDOC>();
                SubscriberSDOC doc = _mapper.Map<SubscriberSDOC>(subscriber);
                doc.SearchAction = cmd;
                docs.value.Add(doc);
                string Json = Newtonsoft.Json.JsonConvert.SerializeObject(docs);
                SendSearchRequest(index, Json);         
            });
            return true;
        }
        
        private async Task<bool> SendSearchRequest(string indexName, string jsonDocs )
        {
  
            string SearchBaseUrl = _configuration["AzureSearch:SearchServiceBaseUrl"];
            string SearchIndexVersion = _configuration["AzureSearch:SearchServiceAdminVersion"];
            string SearchApiKey = _configuration["AzureSearch:SearchServiceAdminApiKey"];            

            string Url = $"{SearchBaseUrl}/indexes/{indexName}/docs/index?api-version={SearchIndexVersion}";


            _logger.LogInformation($"AzureSearchService:SendSearchRequest: url = {Url}");



            HttpClient client = _httpClientFactory.CreateClient(Constants.HttpPostClientName);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Url)
            {
                Content = new StringContent(jsonDocs)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Headers.Add("api-key", SearchApiKey);

            HttpResponseMessage SearchResponse = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(request));
            string ResponseJson = AsyncHelper.RunSync<string>(() => SearchResponse.Content.ReadAsStringAsync());
    
            _logger.LogInformation($"AzureSearchService:SendSearchRequest: json = {jsonDocs}");
            _logger.LogInformation($"AzureSearchService:SendSearchRequest: response = {ResponseJson}");

            if ( SearchResponse.StatusCode == System.Net.HttpStatusCode.OK)
                return true;
            else
                return false;
          
        }



        #endregion


    }
}
