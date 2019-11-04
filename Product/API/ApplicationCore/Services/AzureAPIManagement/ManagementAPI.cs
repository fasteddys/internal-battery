using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;

namespace UpDiddyApi.ApplicationCore.Services.AzureAPIManagement
{
    public class ManagementAPI : IAPIGateway
    {
        private HttpClient _http;
        private IConfiguration _config;
        private IDistributedCache _cache;

        private const string TOKEN_CACHE_KEY = "AzureAPIManagementToken";

        private string _baseUrl;
        private string _version;
        private string _key;
        private string _identifier;
        private string _migrationLoginUrl;
        private string _migrationClientId;

        public ManagementAPI(HttpClient client, IConfiguration configuration, IDistributedCache distributedCache)
        {
            _http = client;
            _cache = distributedCache;

            _baseUrl = configuration["APIGateway:BaseUrl"].TrimEnd('/');
            _version = configuration["APIGateway:Version"];
            _key = configuration["APIGateway:Key"];
            _identifier = configuration["APIGateway:Id"];
            _migrationLoginUrl = configuration["AzureAdB2C:MigrationLoginUrl"];
            _migrationClientId = configuration["AzureAdB2C:MigrationClientId"];
        }

        public async Task<string> GetUserIdAsync(string subscriptionId, string key)
        {
            Subscription subscription = await GetRequest<Subscription>(string.Format("/subscriptions/{0}?api-version={1}", subscriptionId, _version));
            if (subscription.PrimaryKey != key && subscription.SecondaryKey != key)
                return null;

            return subscription.GetUserId();
        }
        public async Task<User> GetUserAsync(string userId)
        {
            User user = await GetRequest<User>(string.Format("/users/{0}?api-version={1}", userId, _version));
            return user;
        }

        public async Task<bool> CheckADB2CLogin(string username, string password)
        {
            bool isValidLogin = false;

            using (HttpClient client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", _migrationClientId),
                    new KeyValuePair<string, string>("scope", "openid " + _migrationClientId + " offline_access"),
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("response_type", "token id_token")
                });

                var response = await client.PostAsync(_migrationLoginUrl, content);
                if (response.StatusCode == HttpStatusCode.OK)
                    isValidLogin = true;
            }

            return isValidLogin;
        }

        private async Task<T> GetRequest<T>(string apiMethod)
        {
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(string.Format("{0}{1}", _baseUrl, apiMethod)),
                Method = HttpMethod.Get,
                Headers =
                {
                    { HttpRequestHeader.Authorization.ToString(), await GetTokenAsync()}
                }
            };

            HttpResponseMessage response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new Exception(response.ReasonPhrase);

            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }

        private async Task<string> GetTokenAsync()
        {
            string token = await _cache.GetStringAsync(TOKEN_CACHE_KEY);

            if (token != null)
                return token;

            var expiry = DateTime.UtcNow.AddDays(1);
            using (var encoder = new HMACSHA512(Encoding.UTF8.GetBytes(_key)))
            {
                var dataToSign = _identifier + "\n" + expiry.ToString("O", CultureInfo.InvariantCulture);
                var hash = encoder.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));
                var signature = Convert.ToBase64String(hash);
                var encodedToken = string.Format("SharedAccessSignature uid={0}&ex={1:o}&sn={2}", _identifier, expiry, signature);
                byte[] encodedTokenBytes = Encoding.UTF8.GetBytes(encodedToken);
                await _cache.SetAsync(TOKEN_CACHE_KEY, encodedTokenBytes, new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(8)));
                return encodedToken;
            }
        }
    }
}
