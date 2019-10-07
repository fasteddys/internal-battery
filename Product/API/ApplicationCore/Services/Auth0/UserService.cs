using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using UpDiddyApi.ApplicationCore.Services.Auth0.Interfaces;
using UpDiddyLib.Shared;

namespace UpDiddyApi.ApplicationCore.Services.Auth0
{
    public class UserService : IUserService
    {
        private const string _CACHE_KEY = "AUTH0_ACCESS_TOKEN";
        private const string _CONNECTION_TYPE = "Username-Password-Authentication";
        private Dictionary<string, string> _tokenParameters = null;
        private readonly string _domain = null;
        private readonly Uri _managementApiUrl = null;
        private readonly string _auth0CryptoKey = null;
        private readonly string _cryptoKey = null;
        private readonly IMemoryCache _memoryCache;

        public UserService(IConfiguration configuration, IMemoryCache memoryCache)
        {
            _tokenParameters = new Dictionary<string, string> {
                { "client_id", configuration["Auth0:ManagementApi:client_id"] },
                { "client_secret", configuration["Auth0:ManagementApi:client_secret"]},
                { "audience", configuration["Auth0:ManagementApi:audience"] },
                { "grant_type", configuration["Auth0:ManagementApi:grant_type"]}
            };
            _domain = configuration["Auth0:Domain"];
            _managementApiUrl = new Uri(configuration["Auth0:ManagementApi:url"]);
            _auth0CryptoKey = configuration["Auth0:CryptoKey"];
            _cryptoKey = configuration["Crypto:Key"];
            _memoryCache = memoryCache;
        }

        private async Task<string> GetApiTokenAsync()
        {
            string encryptedToken = null;
            if (!_memoryCache.TryGetValue<string>(_CACHE_KEY, out encryptedToken))
            {
                string apiResponse = null;
                using (var client = new HttpClient())
                {
                    var encodedContent = new FormUrlEncodedContent(_tokenParameters);
                    using (var response = await client.PostAsync(_managementApiUrl, encodedContent))
                    {
                        apiResponse = await response.Content.ReadAsStringAsync();
                        // todo: handle invalid token here?
                    }
                }
                JObject json = JObject.Parse(apiResponse);
                encryptedToken = (string)json["token"];
            }

            return Crypto.Decrypt(_cryptoKey, encryptedToken);
        }

        public async Task<AccessTokenResponse> CreateUserAsync(User user, params Role[] userRoles)
        {
            var managementApiClient = new ManagementApiClient(await GetApiTokenAsync(), _domain);
            UserCreateRequest userCreationRequest = new UserCreateRequest()
            {
                Email = user.EmailAddress,
                Connection = _CONNECTION_TYPE,
                Password = Crypto.Decrypt(_cryptoKey, user.Password),
                VerifyEmail = true,
                AppMetadata = new JObject()
            };
            Guid subscriberGuid = Guid.NewGuid();
            userCreationRequest.AppMetadata.subscriberGuid = subscriberGuid;
            var userCreationResponse = await managementApiClient.Users.CreateAsync(userCreationRequest);

            throw new NotImplementedException();
        }

        public async Task<User> FindByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }
    }
}
