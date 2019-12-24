using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace API.Tests
{

    public class AzureAPITests
    {
        private static async Task DoLogin()
        {
            var username = "delorean6976+staging10@gmail.com";
            var password = "";

            AuthenticationApiClient _auth0Client = new AuthenticationApiClient(new Uri($"https://stagingcareercircle.auth0.com/"));
            var result = await _auth0Client.GetTokenAsync(new ResourceOwnerTokenRequest
            {
                ClientId = "QVR6E72QfZz9gCHkc3Ig4bDy4n3ufind",
                ClientSecret = "NhSdZanEfJxiYiYmfBFK15BGfZreF0Kk8uXEg7bPUVzWpIREZJbYaymRRKhDRFJU",
                Scope = "openid profile email",
                Realm = "Username-Password-Authentication",
                Username = username,
                Password = password,
                Audience = "https://staging-careercircle.azure-api.net"
            });

            var user = await _auth0Client.GetUserInfoAsync(result.AccessToken);
            var firstClaimValue = user.AdditionalClaims.Where(x => x.Key == ClaimTypes.NameIdentifier).FirstOrDefault();
            Guid subscriberGuid;
            if (firstClaimValue.Value != null && Guid.TryParse(firstClaimValue.Value.ToString(), out subscriberGuid))
            {
                var expiresOn = DateTime.UtcNow.AddSeconds(result.ExpiresIn);

                List<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, subscriberGuid.ToString()));
                claims.Add(new Claim(ClaimTypes.Name, user.FullName));
                claims.Add(new Claim("access_token", result.AccessToken));
                claims.Add(new Claim(ClaimTypes.Expiration, expiresOn.ToString()));

                var permissionClaim = user.AdditionalClaims.Where(x => x.Key == ClaimTypes.Role).FirstOrDefault().Value.ToList();
                string permissions = string.Empty;

                foreach (var permission in permissionClaim)
                {
                    claims.Add(new Claim(ClaimTypes.Role, permission.ToString(), ClaimValueTypes.String, "stagingcareercircle.auth0.com"));
                }

                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

                //await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
                //await _api.UpdateLastSignIn(subscriberGuid);
            }
            else
            {
                throw new ApplicationException("Unable to identify the subscriber.");
            }
        }


        private static string UrlCombine (string url1, string url2)
        {
            if (url1.Length == 0)
                return url2;

            if (url2.Length == 0)
                return url1;

            url1 = url1.TrimEnd('/');
            url2 = url2.TrimStart('/');
            return $"{url1}/{url2}";
        }

        [Fact]
        public void ValidateAzureAPIEndpoints()
        {
            // todo: make tests data-driven: https://ikeptwalking.com/writing-data-driven-tests-using-xunit/

            // todo: configure test user for the target environment and retrieve a valid JWT
            DoLogin().Wait();

            // todo: load the Azure Api subscription key

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "API.Tests.TestFiles.AzureApi-OpenApi.json";
            JObject openApiContent;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string content = reader.ReadToEnd();
                    openApiContent = JObject.Parse(content);
                }
            }

            var serviceUrl = openApiContent.Root.SelectToken("$.serviceUrl");
            var apiVersion = openApiContent.Root.SelectToken("$.apiVersion");
            var path = openApiContent.Root.SelectToken("$.path");
            var subscriptionKeyParameterNames = openApiContent.Root.SelectToken("$.path"); // don't think we need this

            // get list of api endpoints
            var operations = openApiContent.Root.SelectTokens("$.operations.value");

            // schemas contain definitions for all operations (use these to compare responses from service requests)
            var schemas = openApiContent.Root.SelectTokens("$.schemas.value");

            // assume versioning scheme will be segment (do not need to support header or query string right now)
            var baseUrl = UrlCombine(serviceUrl.Value<string>(), apiVersion.Value<string>());

            // call each endpoint
            // todo: make these async?       
            foreach (var operation in operations.Children<JToken>())
            {
                // check to see if a 401 response is possible. if it is, we need to make two calls to this endpoint:
                // one without a valid jwt (to ensure that the endpoint is secured)
                // one with a valid jwt (to ensure that the endpoint works)
                bool isRequiresSubscriberJwt = false;

                if (operation.SelectToken("$.responses[?(@.statusCode == 404)]") != null)
                    isRequiresSubscriberJwt = true;

                var method = operation.SelectToken("$.method").Value<string>();
                var urlTemplate = operation.SelectToken("$.urlTemplate").Value<string>();

                // does the url template contain a replacement value? if so, how do we come up with the replacement value?
                // TODO: map all of the tokens to values from the target environment... e.g., retrieve a valid job guid and replace it whenever we see a {job} url parameter



                WebRequest request = WebRequest.Create(UrlCombine(baseUrl, urlTemplate));
                request.Method = method;

            }

            // todo: test stuff
            Assert.True(true);
        }
    }
}
