using AutoMapper;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Helpers.GoogleProfile;
using System.Net;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace UpDiddyApi.ApplicationCore.Services.GoogleProfile
{
    public class GoogleProfileService : BusinessVendorBase, IGoogleProfileService
    {
     
        #region Class


        public GoogleProfileService(UpDiddyDbContext context, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger sysLog, IHttpClientFactory httpClientFactory)
        {
            _db = context;
            _mapper = mapper;
            _apiBaseUri = configuration["CloudTalent:ProfileBaseUrl"]; 
            _syslog = sysLog;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        #endregion


        #region Profile Tenants

        public BasicResponseDto TenantList(ref string errorMsg)
        {
            BasicResponseDto Rval = null;
            try
            {
            
                string ResponseJson = string.Empty;
                ExecuteProfileApiGet("tenant", ref ResponseJson);
                Rval = Newtonsoft.Json.JsonConvert.DeserializeObject<BasicResponseDto>(ResponseJson);
                if (Rval.StatusCode != 200)
                    throw new Exception(Rval.Description);
            }
            catch (Exception e)
            {
                Rval = new BasicResponseDto()
                {
                    StatusCode = 400,
                    Description = e.Message
                };
                _syslog.Log(LogLevel.Error, $"GoogleProfileInterface: Error listing tentants ", e.Message);
                errorMsg = e.Message;
            }
            return Rval;
        }

        #endregion


        #region Profile Search
        public BasicResponseDto Search(SearchProfilesRequest searchRequest, ref string errorMsg)
        {
            BasicResponseDto Rval = null;
            try
            {
                string Json = Newtonsoft.Json.JsonConvert.SerializeObject(searchRequest);
                string ResponseJson = string.Empty;
                ExecuteProfileApiGet("profile-search", Json, ref ResponseJson);
                Rval = Newtonsoft.Json.JsonConvert.DeserializeObject<BasicResponseDto>(ResponseJson);
                if (Rval.StatusCode != 200)
                    throw new Exception(Rval.Description);
            }
            catch (Exception e)
            {
                Rval = new BasicResponseDto()
                {
                    StatusCode = 400,
                    Description = e.Message
                };
                _syslog.Log(LogLevel.Error, $"GoogleProfileInterface:Search error searching profiles", searchRequest);
                errorMsg = e.Message;
            }
            return Rval;
        }

        #endregion

        #region Profile Crud 
        public BasicResponseDto AddProfile(GoogleCloudProfile googleCloudProfile, ref string errorMsg)
        {
            BasicResponseDto Rval = null;
            try
            {
                // use camel case serializers since the profile uses google wellknown type TimeStamp which has "Seconds" and "Nanos"
                // for properties but google profiles is looking for "seconds" and "nanos"
                string Json = Newtonsoft.Json.JsonConvert.SerializeObject(googleCloudProfile, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                string ResponseJson = string.Empty;
                ExecuteProfileApiPost("profile", Json, ref ResponseJson);
                Rval = Newtonsoft.Json.JsonConvert.DeserializeObject<BasicResponseDto>(ResponseJson);
                if (Rval.StatusCode != 200)
                    throw new Exception(Rval.Description);
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Error, $"GoogleProfileInterface error: {e.Message} while adding google profile for {googleCloudProfile.externalId}", googleCloudProfile);
                errorMsg = e.Message; 
            }
            return Rval;
        }



        public bool UpdateProfile(GoogleCloudProfile googleCloudProfile, ref string  errorMsg)
        {
            bool Rval = true;
            try
            {
                // use camel case serializers since the profile uses google wellknown type TimeStamp which has "Seconds" and "Nanos"
                // for properties but google profiles is looking for "seconds" and "nanos"
                string Json = Newtonsoft.Json.JsonConvert.SerializeObject(googleCloudProfile, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                string ResponseJson = string.Empty;
                ExecuteProfileApiPut("profile", Json, ref ResponseJson);
                var ResponseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<BasicResponseDto>(ResponseJson);                
                if (ResponseObject.StatusCode != 200)
                    throw new Exception(ResponseObject.Description); 
            }
            catch (Exception e) {
                _syslog.Log(LogLevel.Error, $"GoogleProfileInterface error updating google profile for {googleCloudProfile.externalId}", googleCloudProfile);
                errorMsg = e.Message;
                Rval = false;
            }
            return Rval;
        }

        public BasicResponseDto GetProfile(string profileUri, ref string errorMsg)
        {
            BasicResponseDto Rval = null;
            try
            {
                string ResponseJson = string.Empty;         
                string encodedUri = WebUtility.UrlEncode(profileUri);
                ExecuteProfileApiGet($"profile\\{encodedUri}",  ref ResponseJson);
                Rval = Newtonsoft.Json.JsonConvert.DeserializeObject<BasicResponseDto>(ResponseJson);
                if (Rval.StatusCode != 200)
                    throw new Exception(Rval.Description);
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Error, $"GoogleProfileInterface error getting  google profile for {profileUri}", profileUri);
                errorMsg = e.Message;
            }
            return Rval;
        }

        public BasicResponseDto DeleteProfile(string profileUri, ref string errorMsg)
        {
            BasicResponseDto Rval = null;
            try
            {
                string ResponseJson = string.Empty;
                string encodedUri = WebUtility.UrlEncode(profileUri);
                ExecuteProfileApiDelete($"profile\\{encodedUri}", ref ResponseJson);
                Rval = Newtonsoft.Json.JsonConvert.DeserializeObject<BasicResponseDto>(ResponseJson);
                if (Rval.StatusCode != 200)
                    throw new Exception(Rval.Description);
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Error, $"GoogleProfileInterface error deleting google profile for {profileUri}", profileUri);
                errorMsg = e.Message;
            }
            return Rval;
        }

        #endregion

        #region Utility Functions

        private HttpResponseMessage ExecuteProfileApiPut(string ApiAction, string Content, ref string ResponseJson)
        {
            HttpClient client = CreateProfileApiPutClient();
            HttpRequestMessage ProfileApiRequest = CreateProfileApiPutRequest(ApiAction, Content);
            HttpResponseMessage ProfileApiResponse = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(ProfileApiRequest));
            ResponseJson = AsyncHelper.RunSync<string>(() => ProfileApiResponse.Content.ReadAsStringAsync());
            return ProfileApiResponse;

        }


        private HttpResponseMessage ExecuteProfileApiPost(string ApiAction, string Content, ref string ResponseJson)
        {
            HttpClient client = CreateProfileApiPostClient();
            HttpRequestMessage ProfileApiRequest = CreateProfileApiPostRequest(ApiAction, Content);
            HttpResponseMessage ProfileApiResponse = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(ProfileApiRequest));
            ResponseJson = AsyncHelper.RunSync<string>(() => ProfileApiResponse.Content.ReadAsStringAsync());
            return ProfileApiResponse;

        }

        private HttpResponseMessage ExecuteProfileApiGet(string ApiAction, ref string ResponseJson)
        {

            HttpClient client = CreateProfileApiGetClient();
            HttpRequestMessage ProfileApiRequest = CreateProfileApiGetRequest(ApiAction);
            HttpResponseMessage ProfileApiResponse = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(ProfileApiRequest));
            ResponseJson = AsyncHelper.RunSync<string>(() => ProfileApiResponse.Content.ReadAsStringAsync());
            return ProfileApiResponse;
        }

        private HttpResponseMessage ExecuteProfileApiGet(string ApiAction, string Content, ref string ResponseJson)
        {

            HttpClient client = CreateProfileApiGetClient();
            HttpRequestMessage ProfileApiRequest = CreateProfileApiGetRequest(ApiAction);
            ProfileApiRequest.Content = new StringContent(Content);
            ProfileApiRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage ProfileApiResponse = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(ProfileApiRequest));
            ResponseJson = AsyncHelper.RunSync<string>(() => ProfileApiResponse.Content.ReadAsStringAsync());
            return ProfileApiResponse;
        }



        private HttpResponseMessage ExecuteProfileApiDelete(string ApiAction, ref string ResponseJson)
        {

            HttpClient client = CreateProfileApiDeleteClient();
            HttpRequestMessage ProfileApiRequest = CreateProfileApiDeleteRequest(ApiAction);
            HttpResponseMessage ProfileApiResponse = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(ProfileApiRequest));
            ResponseJson = AsyncHelper.RunSync<string>(() => ProfileApiResponse.Content.ReadAsStringAsync());
            return ProfileApiResponse;
        }

        private HttpRequestMessage CreateProfileApiPutRequest(string ApiAction, string Content)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, _apiBaseUri + ApiAction)
            {
                Content = new StringContent(Content)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return request;
        }

        private HttpRequestMessage CreateProfileApiPostRequest(string ApiAction, string Content)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _apiBaseUri + ApiAction)
            {
                Content = new StringContent(Content)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return request;
        }

        private HttpRequestMessage CreateProfileApiGetRequest(string ApiAction)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _apiBaseUri + ApiAction);
            return request;

        }

        private HttpRequestMessage CreateProfileApiDeleteRequest(string ApiAction)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, _apiBaseUri + ApiAction);
            return request;

        }


        private HttpClient CreateProfileApiPutClient()
        {
            HttpClient client = _httpClientFactory.CreateClient(Constants.HttpPutClientName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            return client;

        }

        private HttpClient CreateProfileApiPostClient()
        {
            HttpClient client = _httpClientFactory.CreateClient(Constants.HttpPostClientName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            return client;

        }

        private HttpClient CreateProfileApiGetClient()
        {
            HttpClient client = _httpClientFactory.CreateClient(Constants.HttpGetClientName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            return client;
        }

        private HttpClient CreateProfileApiDeleteClient()
        {
            HttpClient client = _httpClientFactory.CreateClient(Constants.HttpDeleteClientName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            return client;
        }



    }

    #endregion


}
