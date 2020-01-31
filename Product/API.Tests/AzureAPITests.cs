using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json;
using API.Tests.Helpers;
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
            /* todo: 
             * call each endpoint 
             * assert actual response schema (not the data) against the intended response schema
             * assert intended response status code to the actual response status code
             * ensure that unit test responses contain the necessary information to take action if necessary
             */

            Assert.True(true);
        }
    }

    public class ApiEndpointTest
    {
        public Uri Uri { get; set; }
        public string HttpMethod { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public string RequestBody { get; set; }
        public string RequestSchema { get; set; }
        public string ResponseBody { get; set; }
        public string ResponseSchema { get; set; }
        public int StatusCode { get; set; }
        public List<string> DefinitionErrors { get; set; } = new List<string>();
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
        /// Using a test subscriber for the current environment, authenticates with Auth0 and retrieves a valid JWT
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetValidJwtFromTestSubscriber()
        {
            var configuration = ConfigurationHelper.GetConfiguration(Directory.GetCurrentDirectory());

            AuthenticationApiClient _auth0Client = new AuthenticationApiClient(new Uri(configuration["Auth0:Url"].ToString()));
            var result = await _auth0Client.GetTokenAsync(new ResourceOwnerTokenRequest
            {
                ClientId = configuration["Auth0:ClientId"],
                ClientSecret = configuration["Auth0:ClientSecret"],
                Scope = "openid profile email",
                Realm = "Username-Password-Authentication",
                Username = configuration["TestSubscriber:Username"],
                Password = configuration["TestSubscriber:Password"],
                Audience = configuration["Auth0:Audience"]
            });

            return result.AccessToken;
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
            // retrieve a valid Jwt for the test subscriber
            var subscriberJwt = GetValidJwtFromTestSubscriber().Result;

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
            var schemas = openApiSpecification.Root.SelectToken("$.schemas.value[0].document.definitions");

            // call each endpoint   
            foreach (var operation in operations.Children<JToken>())
            {
                var httpMethod = operation.SelectToken("$.method").Value<string>();
                var urlTemplate = operation.SelectToken("$.urlTemplate").Value<string>();
                var operationName = operation.SelectToken("$.name").Value<string>();

                // if the url contains parameters, set it using a test value from the config file
                var replacement = urlParameterReplacementValues
                    .Where(rv => urlTemplate.Contains(rv.Key))
                    .Select(rv => new { Key = rv.Key, Value = rv.Value })
                    .FirstOrDefault();

                if (replacement != null)
                    urlTemplate = urlTemplate.Replace(replacement.Key, replacement.Value);




                // check to see if a 401 response is possible. if it is, we need to add a valid subscriber JWT to the request when testing successful responses
                // todo: one without a valid jwt (to ensure that the endpoint is secured)
                // todo: one with a valid jwt (to ensure that the endpoint works)
                bool isRequiresSubscriberJwtForSuccessfulResponse = (operation.SelectToken("$.responses[?(@.statusCode == 401)]") != null);

                foreach (var response in operation.SelectTokens("$.responses[0]"))
                {
                    ApiEndpointTest apiEndpointTest = new ApiEndpointTest();
                    apiEndpointTest.HttpMethod = httpMethod;
                    apiEndpointTest.Uri = new Uri(Utilities.UrlCombine(baseUrl, urlTemplate));

                    // todo: apiEndpointTest.RequestBody - how will we determine what values to put here - use definition/schema?
                    var rawRequestBody = operation.SelectToken("$.request.representations[0].sample");                    
                    if (rawRequestBody != null && rawRequestBody.HasValues)
                    {
                        try
                        {
                            // todo: parse as json, does that succeed, compare to schema for associated type, is that valid
                            apiEndpointTest.RequestBody = rawRequestBody.Value<string>();                            
                        }
                        catch(Exception e)
                        {
                            apiEndpointTest.DefinitionErrors.Add($"A sample request body was found but could not be parsed; error message: {e.Message}");
                        }
                    }
                        // assume that every request is protected with a subscription
                        apiEndpointTest.Headers.Add(subscriptionKeyParameterKey, subscriptionKeyParameterValue);
                    if (isRequiresSubscriberJwtForSuccessfulResponse)
                        apiEndpointTest.Headers.Add("Authorization", $"Bearer {subscriberJwt}");

                    // use the status code from the response definition as the expected status code for the api endpoint test
                    var statusCode = response.SelectToken("$.statusCode").Value<int>();
                    switch (statusCode)
                    {
                        case 200:
                        case 201:
                        case 202:
                        case 204:
                            apiEndpointTest.StatusCode = statusCode;
                            break;
                        case 401:
                            // remove JWT if it exists. this should cause a 401 response.
                            apiEndpointTest.StatusCode = statusCode;
                            apiEndpointTest.Headers.Remove("Authorization");
                            break;
                        case 400:
                            // TODO: need to think about how to implement tests to specifically test how endpoints handle malformed requests
                            break;
                        default:
                            apiEndpointTest.DefinitionErrors.Add($"Unrecognized response status code: {statusCode.ToString()}");
                            break;
                    }

                    // use the type name from the representation to retrieve the schema, load the example, ensure that the example is valid, then save the schema with the test
                    try
                    {
                        var representation = response.SelectToken("$.representations[0]");
                        if (representation != null)
                        {
                            string typeName = representation.SelectToken("$.typeName").Value<string>();
                            string escapedTypeName = $"['{typeName}']";
                            var responseSchema = schemas.SelectToken($"$.{escapedTypeName}");
                            JSchema jschema = JSchema.Parse(responseSchema.ToString(Formatting.None));
                            JObject example = responseSchema["example"].Value<JObject>();
                            if (!example.IsValid(jschema))
                                throw new ApplicationException("Response example is not valid according to the schema definition.");
                            apiEndpointTest.ResponseSchema = jschema.ToString(SchemaVersion.Unset);
                        }
                    }
                    catch (Exception e)
                    {
                        apiEndpointTest.DefinitionErrors.Add($"Schema not found in response representation; error message: {e.Message}");
                    }

                    // add the api operation to the set of data-driven tests
                    yield return new object[] { $"{operationName} ({statusCode})", new MemberDataSerializer<ApiEndpointTest>(apiEndpointTest) };
                }

            }
        }
    }
}
