using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using UpDiddyApi.ApplicationCore.Interfaces;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class B2CGraphClient : IB2CGraph
    {
        private string clientId { get; set; }
        private string clientSecret { get; set; }
        private string tenant { get; set; }
        private string baseUrl { get; set; }

        private AuthenticationContext authContext;
        private ClientCredential credential;

        private IConfiguration _configuration;
        private HttpClient _http;

        public B2CGraphClient(HttpClient client, IConfiguration configuration)
        {
            // The client_id, client_secret, and tenant are pulled in from the App.config file
            this.clientId = configuration["AzureAdB2C:AppId"];
            this.clientSecret = configuration["AzureAdB2C:AppSecret"];
            this.tenant = configuration["AzureAdB2C:Tenant"];
            this._configuration = configuration;

            // todo: make this a constant url
            // The AuthenticationContext is ADAL's primary class, in which you indicate the directory to use.
            this.authContext = new AuthenticationContext("https://login.microsoftonline.com/" + tenant);

            this.baseUrl = "https://graph.windows.net/" + this.tenant;

            // The ClientCredential is where you pass in your client_id and client_secret, which are 
            // provided to Azure AD in order to receive an access_token using the app's identity.
            this.credential = new ClientCredential(clientId, clientSecret);

            _http = client;
        }

        public async Task<IList<Group>> GetUserGroupsByObjectId(string objectId)
        {
            string response = await SendGraphGetRequest("/users/" + objectId + "/memberOf", null);
            DirectoryObject dirObj = JsonConvert.DeserializeObject<DirectoryObject>(response);
            IList<Group> groups = JsonConvert.DeserializeObject<IList<Group>>(dirObj.AdditionalData["value"].ToString());
            return groups;
        }

        public async Task<User> GetUserBySignInEmail(string email)
        {
            string response = await SendGraphGetRequest("/users", String.Format("$filter=signInNames/any(x:x/value eq '{0}')", HttpUtility.UrlEncode(email)));
            DirectoryObject dirObj = JsonConvert.DeserializeObject<DirectoryObject>(response);
            IList<User> users = JsonConvert.DeserializeObject<IList<User>>(dirObj.AdditionalData["value"].ToString());
            return users.FirstOrDefault();
        }

        public async Task<User> CreateUser(string name, string email, string password)
        {
            var user = new
            {
                displayName = name,
                accountEnabled = true,
                signInNames = new []
                {
                    new
                    {
                        type = "emailAddress",
                        value = email
                    }
                },
                creationType = "LocalAccount",
                passwordProfile = new
                {
                    password = password,
                    forceChangePasswordNextLogin = false
                }
            };

            Dictionary<string, string> b2cExtensionProps = new Dictionary<string, string>();
            b2cExtensionProps.Add(_configuration["AzureAdB2C:ExtensionFields:AgreeToCareerCircleMarketing"],"True");
            b2cExtensionProps.Add(_configuration["AzureAdB2C:ExtensionFields:AgreeToCareerCircleTerms"],_configuration["TermsOfServiceVersion"]);

            var body = JObject.FromObject(user);
            body.Merge(JObject.FromObject(b2cExtensionProps));
            string json = body.ToString();
            string response = await SendGraphPostRequest("/users", json);

            User responseUser = JsonConvert.DeserializeObject<User>(response);
            return responseUser;
        }

        public async Task<string> DisableUser(Guid subscriberGuid)
        {
            // todo: do we care about the response?
            return await SendGraphPatchRequest($"/users/{subscriberGuid}", "{ \"accountEnabled\" : false}");
        }

        public async Task<string> SendGraphGetRequest(string api, string query)
        {
            // First, use ADAL to acquire a token using the app's identity (the credential)
            // The first parameter is the resource we want an access_token for; in this case, the Graph API.
            AuthenticationResult result = await authContext.AcquireTokenAsync("https://graph.windows.net", credential);

            // todo: make this url constant
            string url = this.baseUrl + api + "?api-version=1.6";
            if (!string.IsNullOrEmpty(query))
            {
                url += "&" + query;
            }

            // Append the access token for the Graph API to the Authorization header of the request, using the Bearer scheme.
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpResponseMessage response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                object formatted = JsonConvert.DeserializeObject(error);
                throw new WebException("Error Calling the Graph API: \n" + JsonConvert.SerializeObject(formatted, Formatting.Indented));
            }
            string responseJson = await response.Content.ReadAsStringAsync();
            return responseJson;
        }

        private async Task<string> SendGraphPatchRequest(string api, string json)
        {
            // NOTE: This client uses ADAL v2, not ADAL v4
            AuthenticationResult result = await authContext.AcquireTokenAsync("https://graph.windows.net", credential);
            string url = this.baseUrl + api + "?api-version=1.6";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                object formatted = JsonConvert.DeserializeObject(error);
                throw new WebException("Error Calling the Graph API: \n" + JsonConvert.SerializeObject(formatted, Formatting.Indented));
            }

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> SendGraphPostRequest(string api, string json)
        {
            // NOTE: This client uses ADAL v2, not ADAL v4
            AuthenticationResult result = await authContext.AcquireTokenAsync("https://graph.windows.net", credential);
            string url = this.baseUrl + api + "?api-version=1.6";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                object formatted = JsonConvert.DeserializeObject(error);
                throw new WebException("Error Calling the Graph API: \n" + JsonConvert.SerializeObject(formatted, Formatting.Indented));
            }

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> SendGraphDeleteRequest(string api)
        {
            // NOTE: This client uses ADAL v2, not ADAL v4
            AuthenticationResult result = await authContext.AcquireTokenAsync("https://graph.windows.net", credential);
            string url = this.baseUrl + api + "?api-version=1.6";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpResponseMessage response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                object formatted = JsonConvert.DeserializeObject(error);
                throw new WebException("Error Calling the Graph API: \n" + JsonConvert.SerializeObject(formatted, Formatting.Indented));
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}