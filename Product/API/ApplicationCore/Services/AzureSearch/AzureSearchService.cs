
using AutoMapper;
using GeoJSON.Net.Geometry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Spatial;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
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
 
        public async Task<string> AddOrUpdateG2(G2SDOC g2)
        {
            return await SendG2Request(g2, "upload");
   
        }

        public async Task<string> DeleteG2(G2SDOC g2)
        {
            return await SendG2Request(g2, "delete");
 
        }

        public async Task<string > DeleteG2Bulk(List<G2SDOC> g2s)
        {
            return await SendG2RequestBulk(g2s, "delete");
        }


        public async Task<string > AddOrUpdateG2Bulk(List<G2SDOC> g2s)
        {
            return await SendG2RequestBulk(g2s, "upload");        
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


        private async Task<string> SendG2RequestBulk(List<G2SDOC> g2s, string cmd)
        {
            string index = _configuration["AzureSearch:G2IndexName"];
            SDOCRequest<G2SDOC> docs = new SDOCRequest<G2SDOC>();
            List<Guid> profileGuidList = new List<Guid>();
            foreach ( G2SDOC g2 in g2s)
            {
                profileGuidList.Add(g2.ProfileGuid);
                g2.SearchAction = cmd;
                docs.value.Add(g2);
            }                       

            string Json = Newtonsoft.Json.JsonConvert.SerializeObject(docs, new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ" });
            string statusMsg = string.Empty;
            StrongBox<string> box = new StrongBox<string>(statusMsg);
            // use a strongbox to get the statuse msg box 
            bool rval = await SendSearchIndexRequest(index, cmd, Json,box);
 
        
            return box.Value;
        }


        private async Task<string> SendG2Request(G2SDOC g2, string cmd)
        {            
            string index = _configuration["AzureSearch:G2IndexName"];
            SDOCRequest<G2SDOC> docs = new SDOCRequest<G2SDOC>();                            
            g2.SearchAction = cmd;
            docs.value.Add(g2);
 
            string Json = Newtonsoft.Json.JsonConvert.SerializeObject(docs, new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ" });
            string statusMsg = string.Empty;
            // use a strongbox to get the statuse msg box 
            StrongBox<string> box = new StrongBox<string>(statusMsg);
            bool rval = await SendSearchIndexRequest(index, cmd, Json,box);  
            return box.Value;            
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
                string statusMsg = string.Empty;
                // use a strongbox to get the statuse msg box 
                StrongBox<string> box = new StrongBox<string>(statusMsg);
                SendSearchIndexRequest(index, cmd, Json, box);
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
                string statusMsg = string.Empty;
                // use a strongbox to get the statuse msg box 
                StrongBox<string> box = new StrongBox<string>(statusMsg);
                SendSearchIndexRequest(index, cmd, Json, box);         
            });
            return true;
        }
        
        private async Task<bool> SendSearchIndexRequest(string indexName, string cmd, string jsonDocs, StrongBox<string> statusMsg )
        {
            string ResponseJson = string.Empty;
            try
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
                ResponseJson = AsyncHelper.RunSync<string>(() => SearchResponse.Content.ReadAsStringAsync());

                _logger.LogInformation($"AzureSearchService:SendSearchRequest: json = {jsonDocs}");
                _logger.LogInformation($"AzureSearchService:SendSearchRequest: response = {ResponseJson}");

                // return boolean for overall status and return status msg in the strong box 
                if (SearchResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {                    
                    if (cmd != "delete")
                        statusMsg.Value = $"Indexed On {Utils.ISO8601DateString(DateTime.UtcNow)}";
                    else
                        statusMsg.Value = $"Deleted On {Utils.ISO8601DateString(DateTime.UtcNow)}"; 
                    return true;

                }                    
                else
                {
                    statusMsg.Value = $"StatusCode = {SearchResponse.StatusCode} ResponseJson = {ResponseJson} "; 
                    return false;
                }
                    
            }
            catch ( Exception ex )
            {
                _logger.LogError($"AzureSearchService:SendSearchRequest: Error: {ex.Message}  response = {ResponseJson}");
                return false;

            }
            
          
        }



        #endregion


   
    }
}
