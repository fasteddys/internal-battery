using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class B2CGraphClient : IB2CGraph
    {
        private string clientId { get; set; }
        private string clientSecret { get; set; }
        private string tenant { get; set; }

        private AuthenticationContext authContext;
        private ClientCredential credential;

        private HttpClient _http;

        public B2CGraphClient(HttpClient client, IConfiguration configuration)
        {
            // The client_id, client_secret, and tenant are pulled in from the App.config file
            this.clientId = configuration["AzureAdB2C:AppId"];
            this.clientSecret = configuration["AzureAdB2C:AppSecret"];
            this.tenant = configuration["AzureAdB2C:Tenant"];

            // todo: make this a constant url
            // The AuthenticationContext is ADAL's primary class, in which you indicate the direcotry to use.
            this.authContext = new AuthenticationContext("https://login.microsoftonline.com/" + tenant);

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
        public async Task<User> GetUserByObjectId(string objectId)
        {
            string response = await SendGraphGetRequest("/users/" + objectId, null);
            User user = JsonConvert.DeserializeObject<User>(response);
            return user;
        }

        public async Task<string> AddUserToGroup(string objectId, string groupId)
        {
            string json = $"{{ @odata.id: 'https://graph.microsoft.com/v1.0/users/{objectId}' }}";
            return await SendGraphPostRequest("/groups/" + groupId + "/members/$ref", json);
        }

        public async Task<string> RemoveUserFromGroup(string objectId, string groupId)
        {
            return await SendGraphDeleteRequest("/groups/" + groupId + "/members/" + objectId + "/$ref");
        }

        public async Task<string> SendGraphGetRequest(string api, string query)
        {
            // First, use ADAL to acquire a token using the app's identity (the credential)
            // The first parameter is the resource we want an access_token for; in this case, the Graph API.
            AuthenticationResult result = await authContext.AcquireTokenAsync("https://graph.microsoft.com", credential);

            // todo: make this url constant
            string url = "https://graph.microsoft.com/v1.0" + api;
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

        private async Task<string> SendGraphPostRequest(string api, string json)
        {
            // NOTE: This client uses ADAL v2, not ADAL v4
            AuthenticationResult result = await authContext.AcquireTokenAsync("https://graph.microsoft.com", credential);
            string url = "https://graph.microsoft.com/v1.0" + api;

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
            AuthenticationResult result = await authContext.AcquireTokenAsync("https://graph.microsoft.com", credential);
            string url = "https://graph.microsoft.com/v1.0" + api;
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

        Task<string> IB2CGraph.GetUserByObjectId(string objectId)
        {
            throw new NotImplementedException();
        }
    }
}