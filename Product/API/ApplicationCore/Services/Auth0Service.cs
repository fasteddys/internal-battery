//using System;
//using System.Net.Http;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Configuration;
//using Newtonsoft.Json.Linq;
//using System.Collections.Generic;
//using Microsoft.Extensions.Caching.Memory;
//using Auth0.ManagementApi.Models;
//using Auth0.ManagementApi;
//using UpDiddyApi.ApplicationCore.Interfaces.Business;
//using UpDiddyLib.Dto.Marketing;
//using UpDiddyLib.Shared;
//namespace UpDiddyApi.ApplicationCore.Services
//{
//    public class Auth0Service : IAuth0Service
//    {
//        private readonly IMemoryCache _memoryCache;
//        private const string CACHE_KEY = "AUTH0_ACCESS_TOKEN";
//        private readonly string _clientId = null;
//        private readonly string _clientSecret = null;
//        private readonly string _audience = null;
//        private readonly string _grantType = null;
//        private readonly Uri _url = null;

//        public Auth0Service(IConfiguration config, IMemoryCache memoryCache)
//        {
//            _clientId = config["Auth0:ManagementApi:client_id"];
//            _clientSecret = config["Auth0:ManagementApi:client_secret"];
//            _audience = config["Auth0:ManagementApi:audience"];
//            _grantType = config["Auth0:ManagementApi:grant_type"];
//            _url = new Uri(config["Auth0:ManagementApi:url"]);
//            _memoryCache = memoryCache;
//        }

//        // get a token (from memory cache or from auth0 if it is expired)

//        // create user (this is an action that requires the management api credentials to perform)

//        // login (currently in webapp, needs to be moved)

//        // reset pw

//        // forgot pw

//        // logout






//        /// <summary>
//        ///  Requests a new token for the Auth0 Management API
//        /// </summary>
//        /// <returns></returns>
//        private async Task<string> RequestNewToken()
//        {
//            string token = string.Empty;
//            try
//            {
//                //TODO - store the token and the expiration datetime in memory storage. This needs to be encrypted. 
//                // If their api endpoint is down, retry it for number of times at a fixed interval. If the encryption fails, retrieve the token 
//                // without storing it in the cache.
//                string apiResponse = string.Empty;
//                using (var client = new HttpClient())
//                {

//                    var encodedContent = new FormUrlEncodedContent(_parameters);
//                    using (var response = await client.PostAsync(_url, encodedContent))
//                    {
//                        apiResponse = await response.Content.ReadAsStringAsync();
//                    }
//                }
//                JObject json = JObject.Parse(apiResponse);
//                token = (string)json["token"];
//            }
//            catch (Exception e)
//            {
//                //TODO - Handle exception here
//                return null;
//            }

//            return token;
//        }

//        /// <summary>
//        /// Creats a new user in Auth0
//        /// </summary>
//        /// <param name="signUpDto"></param>
//        /// <returns></returns>
//        public async Task<User> CreateUser(SignUpDto signUpDto)
//        {
//            try
//            {
//                var api = new ManagementApiClient(await RequestNewToken(), _config["Auth0:Domain"]);
//                UserCreateRequest request = new UserCreateRequest()
//                {
//                    Email = signUpDto.email,
//                    Connection = "Username-Password-Authentication",
//                    Password = Crypto.Decrypt(_config["Crypto:Key"], signUpDto.password),
//                    VerifyEmail = false,
//                    AppMetadata = new JObject()

//                };
//                Guid subscriberGuid = Guid.NewGuid();
//                request.AppMetadata.subscriberGuid = subscriberGuid;
//                return await _managementApiClient.Users.CreateAsync(request);
//            }
//            catch (Exception e)
//            {
//                //TODO - Handle exception here
//                return null;
//            }
//        }


//    }
//}