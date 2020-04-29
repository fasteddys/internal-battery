
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Models.Views;
using UpDiddyLib.Domain.Models.HubSpot;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
using Newtonsoft.Json;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class HubSpotService : IHubSpotService
    {

        private readonly UpDiddyDbContext _db = null;
        private readonly ILogger _syslog;
        private IConfiguration _configuration { get; set; }
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IHangfireService _hangfireService;
        private readonly IHttpClientFactory _httpClientFactory;

        public HubSpotService(UpDiddyDbContext db, ILogger<HubSpotService> sysLog, IRepositoryWrapper repositoryWrapper, IHangfireService hangfireService, IConfiguration configuration, IHttpClientFactory httpClientFactory )
        {            
            _db = db;
            _syslog = sysLog;            
            _repositoryWrapper = repositoryWrapper;
            _hangfireService = hangfireService;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }



        public async Task<long> AddOrUpdateContactBySubscriberGuid(Guid subscriberGuid, DateTime? lastLoginDateTime = null, bool nonBlocking = true)
        {
            _syslog.LogInformation($"HubSpotService.AddOrUpdateContactBySubscribterGuid: Starting for subscriber {subscriberGuid} nonBlocking = {nonBlocking}");

            // Fire off background job if non-block has been requested and hangfire is enabled 
            if (nonBlocking && bool.Parse(_configuration["Hangfire:IsProcessingServer"]))
            {
                _syslog.LogInformation($"HubSpotService.AddOrUpdateContactBySubscribterGuid: Background job starting for subscriber {subscriberGuid}");
                _hangfireService.Enqueue<HubSpotService>(j => j._AddOrUpdateContactBySubscriberGuid(subscriberGuid, lastLoginDateTime));
                return 0;
            }
            else
            {
                _syslog.LogInformation($"HubSpotService.AddOrUpdateContactBySubscribterGuid: Invoking non-blocking _AddOrUpdateContact for subscriber {subscriberGuid}");
                long hubSpotVid = await _AddOrUpdateContactBySubscriberGuid(subscriberGuid, lastLoginDateTime);
                _syslog.LogInformation($"HubSpotService.AddOrUpdateContactBySubscribterGuid: Subscriber {subscriberGuid} has hubSpotVid = {hubSpotVid}");
                return hubSpotVid;
            }                            
        }


        // add or update the current subsscriber in hubspot
        public async Task<long> _AddOrUpdateContactBySubscriberGuid(Guid subscriberGuid, DateTime? lastLoginDateTime)
        {
            _syslog.LogInformation($"HubSpotService._AddOrUpdateContact: starting");
            long rval = -1;
            string json = string.Empty;
            try
            {
                Subscriber subscriber = _repositoryWrapper.SubscriberRepository.GetAll()
                        .Include(s1 => s1.SubscriberSkills)
                        .ThenInclude(ss => ss.Skill)
                        .Where(x => x.IsDeleted == 0 && x.SubscriberGuid == subscriberGuid)
                        .FirstOrDefault();

                if (subscriber == null)
                    throw new DllNotFoundException($"HubSpotService._AddOrUpdateContactBySubscribterGuid: cannot locate subscriber {subscriberGuid}");

                // get list of self curated skills as string with new lines 
                string selfCuratedSkills = string.Join("\n", subscriber.SubscriberSkills.Select(u => u.Skill.SkillName).ToArray());

                Guid publicDataCompanyGuid = Guid.Parse(_configuration["CareerCircle:PublicDataCompanyGuid"]);

                // Get all non-public G2s for subscriber 
                var g2Profile = _db.ProfileAzureSearch
                            .Where(p => p.SubscriberGuid == subscriberGuid && p.CompanyGuid != publicDataCompanyGuid)
                            .FirstOrDefault();

                string g2PublicSkills = g2Profile?.PublicSkills.Replace(';', '\n');

                //Get LastResumeUploadDate for subscriber
                var lastResumeUploadDate = await _repositoryWrapper.SubscriberFileRepository.GetMostRecentCreatedDate(subscriberGuid);

                // get the source partner for the subscriber 
                SubscriberSourceDto sourcePartner = await _repositoryWrapper.SubscriberRepository.GetSubscriberSource(subscriber.SubscriberId);

                HubSpotContactDto contact = new HubSpotContactDto()
                {
                    SubscriberGuid = subscriberGuid,
                    FirstName = subscriber.FirstName,
                    LastName = subscriber.LastName,
                    Email = subscriber.Email,
                    HubSpotVid = subscriber.HubSpotVid,
                    DateJoined = subscriber.CreateDate,
                    LastLoginDate = lastLoginDateTime.HasValue ? lastLoginDateTime.Value : subscriber.LastSignIn.Value,
                    SelfCuratedSkills = selfCuratedSkills,
                    SourcePartner = sourcePartner != null ?  sourcePartner.PartnerName : null,
                    SkillsG2 = g2PublicSkills,
                    LastResumeUploadDate = lastResumeUploadDate
                };

                // map the contact dto to a hubspot property dto and serialize it
                json = JsonConvert.SerializeObject(MapToHubSpotPropertiesDto(contact));
                _syslog.LogInformation($"HubSpotService._AddOrUpdateContact: json = {json}");
                string BaseUrl = _configuration["HubSpot:BaseUrl"];
                string ApiKey = _configuration["HubSpot:ApiKey"];
                string Url = BaseUrl + "/contacts/v1/contact/createOrUpdate/email/" + contact.Email + "?hapikey=" + ApiKey;

                // call hubspot  
                HttpClient client = _httpClientFactory.CreateClient(Constants.HttpPostClientName);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Url)
                {
                    Content = new StringContent(json)
                };
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage SearchResponse = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(request));
                string ResponseJson = AsyncHelper.RunSync<string>(() => SearchResponse.Content.ReadAsStringAsync());
                _syslog.LogInformation($"HubSpotService:_AddOrUpdateContactBySubscriberGuid: response = {ResponseJson}");
                // if all went well capture the returned vid   
                if (SearchResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _syslog.LogInformation($"HubSpotService._AddOrUpdateContact: Success HubSpot returned OK");
                    // extract vid from response 
                    JObject responseObject = JObject.Parse(ResponseJson);
                    string vidString = (string)responseObject["vid"];                    
                    rval = long.Parse(vidString);
                    _syslog.LogInformation($"HubSpotService._AddOrUpdateContactBySubscriberGuid: Vid returned {rval}");

                    //Update Hubspotvid
                    await _repositoryWrapper.SubscriberRepository.UpdateHubSpotDetails(subscriberGuid, rval);
                }
                else
                {
                    _syslog.LogError($"HubSpotService._AddOrUpdateContactBySubscriberGuid: See details below;\n\r "+ 
                        $"Hubspot API request: {JsonConvert.SerializeObject(request)}\n\r" +
                        $"Hubspot API http response: {JsonConvert.SerializeObject(SearchResponse) ?? "null"}\n\r" +
                        $"Hubspot API response content: {ResponseJson ?? "null"}");
                    throw new Exception($"HubSpot service failure with statuscode {SearchResponse.StatusCode}");
                }
            }
            catch ( Exception ex )
            {
                _syslog.LogError($"HubSpotService._AddOrUpdateContactBySubscriberGuid: Error {ex.Message}");
            }
            finally
            {
                _syslog.LogInformation($"HubSpotService._AddOrUpdateContactBySubscriberGuid: Done");
            }                                      
            return rval;
        }


        #region private helper functions 

        //todo find a more elegant way to do this mapping when time allows 
        private HubSpotPropertiesDto MapToHubSpotPropertiesDto(HubSpotContactDto hubSpotContactDto)
        {

            HubSpotPropertiesDto rVal = new HubSpotPropertiesDto();

            // ran a test and with property of FIRSTNAME and it worked, so..  hubspot property names are case independent
            HubSpotProperty p = new HubSpotProperty()
            {
                property = "firstname",
                value = hubSpotContactDto.FirstName
            };
            rVal.properties.Add(p);

            p = new HubSpotProperty()
            {
                property = "lastname",
                value = hubSpotContactDto.LastName
            };
            rVal.properties.Add(p);

            // if the value is null, it will null it out in hubspot. 
            p = new HubSpotProperty()
            {
                property = "subscriberguid",
                value = hubSpotContactDto.SubscriberGuid?.ToString()
            };
            rVal.properties.Add(p);

            if ( hubSpotContactDto.DateJoined != null )
            {
                p = new HubSpotProperty()
                {
                    property = "datejoined",
                    value = Utils.ToUnixTimeInMilliseconds(Utils.ToMidnight(hubSpotContactDto.DateJoined.Value)).ToString()
                };
                rVal.properties.Add(p);
            }

            if (hubSpotContactDto.LastLoginDate != null)
            {        
                p = new HubSpotProperty()
                {
                    property = "lastlogin",
                    value = Utils.ToUnixTimeInMilliseconds(Utils.ToMidnight(hubSpotContactDto.LastLoginDate.Value)).ToString()
                };
                rVal.properties.Add(p);
            }

            //LastResumeUploadDate
            if (hubSpotContactDto.LastResumeUploadDate.HasValue)
            {
                p = new HubSpotProperty()
                {
                    property = "lastresumeuploaddate",
                    value = Utils.ToUnixTimeInMilliseconds(Utils.ToMidnight(hubSpotContactDto.LastResumeUploadDate.Value)).ToString()
                };
                rVal.properties.Add(p);
            }

            if ( string.IsNullOrEmpty(hubSpotContactDto.SelfCuratedSkills) == false )
            {
                p = new HubSpotProperty()
                {
                    property = "skillsselfcurated",
                    value = hubSpotContactDto.SelfCuratedSkills
                };
                rVal.properties.Add(p);
            }

            if (!string.IsNullOrEmpty(hubSpotContactDto.SkillsG2))
            {
                p = new HubSpotProperty()
                {
                    property = "skillsg2",
                    value = hubSpotContactDto.SkillsG2
                };
                rVal.properties.Add(p);
            }

            if (string.IsNullOrEmpty(hubSpotContactDto.SourcePartner) == false)
            {
                p = new HubSpotProperty()
                {
                    property = "sourcepartner",
                    value = hubSpotContactDto.SourcePartner
                };
                rVal.properties.Add(p);
            }



            return rVal;
        }

        #endregion


    }
}
