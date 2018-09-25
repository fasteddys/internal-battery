using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.Identity.Client;
using System.Linq;
using UpDiddy.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft;
using Newtonsoft.Json;

namespace UpDiddy.Helpers
{
    public class ApiHelperMsal
    {
        protected IConfiguration _configuration { get; set; }
        protected string _ApiBaseUri = String.Empty;
        public AzureAdB2COptions AzureOptions { get; set; }
        public HttpContext HttpContext { get; set; }
  
        public T Get<T>(string ApiAction, bool Authorized = false)
        {
            Task<string> Response = _GetAsync(ApiAction, Authorized);
            try
            {                
                T rval = JsonConvert.DeserializeObject<T>(Response.Result);
                return rval;             
            }
            catch ( Exception ex )
            {
                // TODO instrument with json string and requested type 
                var msg = ex.Message;
                return (T)Convert.ChangeType(null, typeof(T)); 
            }
        }


        public string GetAsString(string ApiAction, bool Authorized = false)
        {
            Task<string> Response = _GetAsync(ApiAction, Authorized);
            return Response.Result;
        }

        #region Helpers Functions

        private async Task AddBearerTokenAsync(HttpRequestMessage request)
        {
          
            // Retrieve the token with the specified scopes
            var scope = AzureOptions.ApiScopes.Split(' ');
            string signedInUserID = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            TokenCache userTokenCache = new MSALSessionCache(signedInUserID, this.HttpContext).GetMsalCacheInstance();
            ConfidentialClientApplication cca = new ConfidentialClientApplication(AzureOptions.ClientId, AzureOptions.Authority, AzureOptions.RedirectUri, new ClientCredential(AzureOptions.ClientSecret), userTokenCache, null);
            AuthenticationResult result = await cca.AcquireTokenSilentAsync(scope, cca.Users.FirstOrDefault(), AzureOptions.Authority, false);
            // Add Bearer Token for MSAL authenticatiopn
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
        }

        private async Task<string> _GetAsync(string ApiAction, bool Authorized = false)
        {
            string responseString = "";
            try
            {               
                HttpClient client = new HttpClient();
                string ApiUrl = _ApiBaseUri + ApiAction;
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, ApiUrl);

                // Add token to the Authorization header and make the request 
                if ( Authorized)
                    await AddBearerTokenAsync(request);

                HttpResponseMessage response = await client.SendAsync(request);
                // Handle the response
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        responseString = await response.Content.ReadAsStringAsync();
                        break;
                    case HttpStatusCode.Unauthorized:
                        responseString = $"Please sign in again. {response.ReasonPhrase}";
                        break;
                    default:
                        responseString = $"Error calling API. StatusCode=${response.StatusCode}";
                        break;
                }
            }
            catch (MsalUiRequiredException ex)
            {
                responseString = $"Session has expired. Please sign in again. {ex.Message}";
            }
            catch (Exception ex)
            {
                responseString = $"Error calling API: {ex.Message}";
            }

            return responseString;

        }


#endregion

    }
}
