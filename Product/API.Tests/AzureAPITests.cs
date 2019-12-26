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
using System.Collections;
using Xunit.Abstractions;
using Newtonsoft.Json;
using API.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Net.Http;
using System.Security.Authentication;
using Newtonsoft.Json.Schema;

namespace API.Tests.AzureApi
{

    public class DataDrivenApiEndpointTests
    {
        [Theory]
        [MemberData(nameof(AzureApiDataProvider.ParseOpenApiSpecificationForApiEndpoints), MemberType = typeof(AzureApiDataProvider))]
        public void Validate_All_Api_Endpoints_Conform_To_Specifications(string apiEndpointTestName, MemberDataSerializer<ApiEndpointTest> apiEndpointTest)
        {
            Assert.True(true);
        }
    }

    public class ApiEndpointTest
    {
        public Uri Uri { get; set; }
        public string HttpMethod { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public int StatusCode { get; set; }
        public List<string> DefinitionErrors { get; set; }
    }

    /// <summary>
    /// Retrieves all of the Azure API endpoints using Azure Resource Management 
    /// </summary>
    public class AzureApiDataProvider
    {
        public static Dictionary<string, string> GetUrlParameterReplacementValues()
        {
            var urlParameterReplacementValues = new Dictionary<string, string>();
            var configuration = ConfigurationHelper.GetConfiguration(Directory.GetCurrentDirectory());
            
            var configurationSectionWithReplacementValues = configuration.GetSection("UrlParameterReplacementValues").GetChildren();
            foreach (var urlParameterReplacementValue in configurationSectionWithReplacementValues)
            {
                urlParameterReplacementValues.Add(urlParameterReplacementValue.Key, urlParameterReplacementValue.Value);
            }

            return urlParameterReplacementValues;
        }

        /// <summary>
        /// Constructs a Uri which can be used to retrieve the OpenApi specification for an Azure API
        /// </summary>
        /// <returns></returns>
        public static Uri GetOpenApiRequestUri()
        {
            var configuration = ConfigurationHelper.GetConfiguration(Directory.GetCurrentDirectory());

            StringBuilder url = new StringBuilder("https://");
            url.Append(configuration["AzureResourceManagement:ResourceName"]);
            url.Append(".management.azure-api.net/subscriptions/");
            url.Append(configuration["AzureResourceManagement:SubscriptionId"]);
            url.Append("/resourceGroups/");
            url.Append(configuration["AzureResourceManagement:ResourceGroupId"]);
            url.Append("/providers/Microsoft.ApiManagement/service/");
            url.Append(configuration["AzureResourceManagement:ServiceId"]);
            url.Append("/apis/");
            url.Append(configuration["AzureResourceManagement:ApiId"]);
            url.Append("?format=openapi-link-json&export=true&api-version=2019-01-01");
            return new Uri(url.ToString(), UriKind.Absolute);
        }

        /// <summary>
        /// Submits a web request for an Azure Api using Azure Resource Management and returns the response as JSON
        /// </summary>
        /// <param name="openApiRequestUri"></param>
        /// <returns></returns>
        public static JObject LoadOpenApiSpecification(Uri openApiRequestUri)
        {
            HttpClient client = new HttpClient(new HttpClientHandler() { SslProtocols = SslProtocols.Tls12 });
            var response = client.GetAsync(GetOpenApiRequestUri()).Result;
            if (response.StatusCode != HttpStatusCode.OK)
                throw new ApplicationException($"Invalid status code while retrieving Api Endpoints: {response.StatusCode.ToString()}");

            return response.Content.ReadAsAsync<JObject>().Result;
        }

        /// <summary>
        /// Retrieves a valid subscription key value for use with Azure Api
        /// </summary>
        /// <returns></returns>
        public static string GetSubscriptionKeyValue()
        {
            var configuration = ConfigurationHelper.GetConfiguration(Directory.GetCurrentDirectory());
            return configuration["SubscriptionKeyValue"].ToString();
        }

        /// <summary>
        /// Parses the OpenApi specification and constructs tests for all Api endpoints  
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> ParseOpenApiSpecificationForApiEndpoints()
        {            
            // load the OpenApi specification from Azure Resource Management
            JObject openApiSpecification = LoadOpenApiSpecification(GetOpenApiRequestUri());

            // get all url replacement values from app settings for testing entity-specific endpoints
            Dictionary<string, string> urlParameterReplacementValues = GetUrlParameterReplacementValues();

            // construct the base url that will be used for all api endpoint tests (do not need to support header or query string right now)
            string serviceUrl = openApiSpecification.Root.SelectToken("$.serviceUrl").Value<string>();
            string path = openApiSpecification.Root.SelectToken("$.path").Value<string>();
            string apiVersion = openApiSpecification.Root.SelectToken("$.apiVersion").Value<string>();
            string baseUrl = Utilities.UrlCombine(Utilities.UrlCombine(serviceUrl, path), apiVersion);

            // get valid subscription for use with the Azure Api - this needs to be added to every request
            var subscriptionKeyParameterKey = openApiSpecification.Root.SelectToken("$.subscriptionKeyParameterNames.header").Value<string>();
            string subscriptionKeyParameterValue = GetSubscriptionKeyValue();

            // get list of api endpoints
            var operations = openApiSpecification.Root.SelectTokens("$.operations.value");

            // schemas contain definitions for all operations (use these to compare responses from service requests)
            var schemas = openApiSpecification.Root.SelectTokens("$.schemas.value");

            // call each endpoint   
            foreach (var operation in operations.Children<JToken>())
            {
                var httpMethod = operation.SelectToken("$.method").Value<string>();
                var urlTemplate = operation.SelectToken("$.urlTemplate").Value<string>();



                // does the url template contain a replacement value? if so, how do we come up with the replacement value?
                // TODO: map all of the tokens to values from the target environment... e.g., retrieve a valid job guid and replace it whenever we see a {job} url parameter



                //WebRequest request = WebRequest.Create(UrlCombine(baseUrl, urlTemplate));
                //request.Method = method;



                // check to see if a 401 response is possible. if it is, we need to add a valid subscriber JWT to the request when testing successful responses
                // one without a valid jwt (to ensure that the endpoint is secured)
                // one with a valid jwt (to ensure that the endpoint works)
                bool isRequiresSubscriberJwtForSuccessfulResponse = (operation.SelectToken("$.responses[?(@.statusCode == 401)]") != null);

                foreach (var response in operation.SelectTokens("$.responses"))
                {
                    ApiEndpointTest apiEndpointTest = new ApiEndpointTest();
                    
                    // assume that every request is protected with a subscription
                    apiEndpointTest.Headers.Add(subscriptionKeyParameterKey, subscriptionKeyParameterValue);

                    // use the status code from the response definition as the expected status code for the api endpoint test
                    var statusCode = response.SelectToken("$.statusCode").Value<int>();
                    switch (statusCode)
                    {
                        case 200:
                        case 201:
                        case 202:
                        case 204:
                        case 401:
                            apiEndpointTest.StatusCode = statusCode;
                            break;
                        case 400:
                            // TODO: need to think about how to implement tests to specifically test how endpoints handle malformed requests
                            break;
                        default:
                            apiEndpointTest.DefinitionErrors.Add($"Unrecognized response status code: {statusCode.ToString()}");
                            break;
                    }

                    // JsonSchema.Parse() -- todo: how should we store this?
                    yield return new object[] { "replace this with the name of the operation", new MemberDataSerializer<ApiEndpointTest>(apiEndpointTest) };
                }

                
            }

            // todo: remove these
            yield return new object[] { "apiEndpoint1", new MemberDataSerializer<ApiEndpointTest>(new ApiEndpointTest { HttpMethod = "GET", StatusCode = 200 }) };
            yield return new object[] { "apiEndpoint2", new MemberDataSerializer<ApiEndpointTest>(new ApiEndpointTest { HttpMethod = "POST", StatusCode = 201 }) };
        }
    }


    /*
    public class AzureAPITests
    {
        private static async Task DoLogin()
        {
            var username = "delorean6976+staging10@gmail.com";
            var password = "Temporary1";

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


        private static string UrlCombine(string url1, string url2)
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
    */
}
