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
using System.Net.Http;
using System.Text;

namespace UpDiddyApi.ApplicationCore.Services
{

    class HiringSolvedJson
    {
        public string name { get; set; }
        public string data { get; set; }

    }


    public class HiringSolvedService : IHiringSolvedService
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
        private readonly IHttpClientFactory _httpClientFactory;


        private HttpClient Client { get; }



        public HiringSolvedService(UpDiddyDbContext context,
            IConfiguration configuration,
            ICloudStorage cloudStorage,
            IRepositoryWrapper repository,
            ILogger<SubscriberService> logger,
            IMapper mapper,
            ITaggingService taggingService,
            IHangfireService hangfireService,
            IFileDownloadTrackerService fileDownloadTrackerService,
            ISysEmail sysEmail,
            IButterCMSService butterCMSService,
            IHttpClientFactory httpClientFactory)
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
            _httpClientFactory = httpClientFactory;


            Client = _httpClientFactory.CreateClient(Constants.HttpPostClientName); 
            Client.DefaultRequestHeaders.Add("x-api-key", configuration["HiringSolved:ServiceKey"]);
        }



        public async Task<bool> RequestParse(int subscriberId, string fileName, string resume64Encoded)
        {

            _logger.LogInformation($"HiringSolvedService:RequestParse SubscriberId = {subscriberId}");
            try
            {

                HiringSolvedJson payload = new HiringSolvedJson
                {
                    name = fileName,
                    data = resume64Encoded
                };

                string Json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);                
                HttpContent content = new StringContent(Json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await Client.PostAsync(_configuration["HiringSolved:BaseUrl"], content);

                var ResponseJson =  await response.Content.ReadAsStringAsync();
                var responseStream =  await new StreamReader(await response.Content.ReadAsStreamAsync()).ReadToEndAsync();

                // create parse record 
                HiringSolvedResumeParse newParse = new HiringSolvedResumeParse()
                {
                    CreateDate = DateTime.UtcNow,
                    ParseRequested = DateTime.UtcNow,
                    FileName = fileName,
                    SubscriberId = subscriberId,
                    HiringSolvedResumeParseGuid = Guid.NewGuid(),
                    CreateGuid = Guid.Empty,
                    IsDeleted = 0,    
                    ResumeText = resume64Encoded                                                
                };

                // get parse status and job id from hiringSolved
                 var ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);
                newParse.ParseStatus = ResponseObject.status.ToObject<string>();
                newParse.JobId = ResponseObject.job_id.ToObject<string>();
 
                await _repository.HiringSolvedResumeParseRepository.Create(newParse);
                await _repository.HiringSolvedResumeParseRepository.SaveAsync();
                _logger.LogInformation($"HiringSolvedService:RequestParse Done with status of {response.StatusCode}");
            }
            catch ( Exception ex )
            {
                _logger.LogError($"HiringSolvedService:RequestParse Error {ex.Message} ");
            }
        
            return true; 
        }

        public async Task<bool> GetParseStatus(string JobId )
        {

            _logger.LogInformation($"HiringSolvedService:RequestParseStatus JobId = {JobId}");
            try
            {
                // Get the parse record 
                HiringSolvedResumeParse parseRequest =  _repository.HiringSolvedResumeParseRepository.GetAllWithTracking()
                    .Where(p => p.IsDeleted == 0 && p.JobId == JobId)
                    .FirstOrDefault();

                if ( parseRequest == null )
                    throw new NotFoundException($"HiringSolvedService:RequestParseStatus Cannot locate job {JobId}");

                // Get status from hiring solved 
                string requestUrl = $"{_configuration["HiringSolved:BaseUrl"]}/{JobId}"; 
                HttpResponseMessage response = await Client.GetAsync(requestUrl);
                string parseStatus = string.Empty;
                var ResponseJson = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"HiringSolvedService:RequestParse ResponseJson = {ResponseJson}");
                if ( response.StatusCode == System.Net.HttpStatusCode.OK )
                {
                    var ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ResponseJson);

                    parseStatus = ResponseObject.status.ToObject<string>();
                    if (parseStatus == Constants.HiringSolvedStatus.Finished)
                    {
                        JObject resultsJObject = ResponseObject.results.ToObject<JObject>();
                        string results = resultsJObject.ToString(Formatting.None);
                        parseRequest.ParsedResume = results;
                        DateTime parseCompletedTime = DateTime.UtcNow;
                        parseRequest.ParseCompleted = parseCompletedTime;
                        parseRequest.ParseStatus = parseStatus;
                        TimeSpan ts = parseCompletedTime - parseRequest.ParseRequested.Value;
                        parseRequest.NumTicks = ts.Ticks;
                    }                          
                }
                else
                {
                    parseStatus = $"Failed response = {ResponseJson}";
                }
                       
                await _repository.HiringSolvedResumeParseRepository.SaveAsync();
                _logger.LogInformation($"HiringSolvedService:RequestParse Done with status of {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"HiringSolvedService:RequestParse Error {ex.Message} ");
            }

            return true;
        }




    }



}
