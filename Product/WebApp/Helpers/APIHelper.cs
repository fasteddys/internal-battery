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

namespace UpDiddy.Helpers
{
    public class APIHelper
    {

        public AzureAdB2COptions AzureOptions { get; set; }
        public HttpContext HttpContext { get; set; }
        public APIHelper(AzureAdB2COptions Options, HttpContext Context)
        {
            AzureOptions = Options;
            HttpContext = Context;
        }

        public async Task<string> HelloTest()
        {
            string responseString = "";
            try
            { 
                // Retrieve the token with the specified scopes
                var scope = AzureOptions.ApiScopes.Split(' ');
                string signedInUserID = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                TokenCache userTokenCache = new MSALSessionCache(signedInUserID, this.HttpContext).GetMsalCacheInstance();
                ConfidentialClientApplication cca = new ConfidentialClientApplication(AzureOptions.ClientId, AzureOptions.Authority, AzureOptions.RedirectUri, new ClientCredential(AzureOptions.ClientSecret), userTokenCache, null);

                AuthenticationResult result = await cca.AcquireTokenSilentAsync(scope, cca.Users.FirstOrDefault(), AzureOptions.Authority, false);

                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, AzureOptions.ApiUrl);

                // Add token to the Authorization header and make the request
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);


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

    }
}
