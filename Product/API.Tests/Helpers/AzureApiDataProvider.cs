using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.Net.Http;
using System.Security.Authentication;

namespace API.Tests.Helpers
{
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
        /// Constructs a Uri which can be used to retrieve the list of Azure Apis that have been published to a service
        /// </summary>
        /// <returns></returns>
        public static Uri GetAzureServiceUri()
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
            url.Append("/apis?api-version=2019-01-01");
            return new Uri(url.ToString(), UriKind.Absolute);
        }

        /// <summary>
        /// Constructs a Uri which can be used to retrieve the OpenApi specification for a particular Azure Api of Azure Apis that have been published to a service
        /// </summary>
        /// <returns></returns>
        public static Uri GetAzureServiceApiUri(string apiName)
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
            url.Append(apiName);
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
            var response = client.GetAsync(openApiRequestUri).Result;
            if (response.StatusCode != HttpStatusCode.OK)
                throw new ApplicationException($"Invalid status code while retrieving Api Endpoints: {response.StatusCode.ToString()}");

            return response.Content.ReadAsAsync<JObject>().Result;
        }

        /// <summary>
        /// Submits a web request to Azure Api Management for all Apis in a specific service. 
        /// The ApiVersionSetId is returned for each current (active) Api in the target service.
        /// </summary>
        /// <param name="azureApimResourceRequestUri"></param>
        /// <returns></returns>
        public static List<Uri> DiscoverActiveApiEndpoints(Uri azureApimResourceRequestUri)
        {
            var apiVersionSetIds = new List<Uri>();
            HttpClient client = new HttpClient(new HttpClientHandler() { SslProtocols = SslProtocols.Tls12 });
            var response = client.GetAsync(azureApimResourceRequestUri).Result;
            if (response.StatusCode != HttpStatusCode.OK)
                throw new ApplicationException($"Invalid status code while retrieving Apis: {response.StatusCode.ToString()}");

            var serviceResponse = response.Content.ReadAsAsync<JObject>().Result;
            var services = serviceResponse.SelectTokens("$.value");
            foreach (var service in services.Children())
            {
                var isCurrent = service.SelectToken("$.properties.isCurrent").Value<bool?>();
                if (isCurrent.HasValue && isCurrent.Value)
                {
                    apiVersionSetIds.Add(GetAzureServiceApiUri(service["name"].Value<string>()));
                }
            }
            return apiVersionSetIds;
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
        /// Discovers all Apis within a service and creates Api operation tests for each one
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> ExtractApiOperationsFromAllApis()
        {
            // retrieve a valid Jwt for the test subscriber
            var subscriberJwt = GetValidJwtFromTestSubscriber().Result;

            // construct the environment-specific url for the azure service which contains the apis to be tested
            Uri azureServiceRequest = GetAzureServiceUri();

            // discover all active Apis in the target service
            List<Uri> apiUris = DiscoverActiveApiEndpoints(azureServiceRequest);

            // get all url replacement values from app settings for testing entity-specific endpoints
            Dictionary<string, string> urlParameterReplacementValues = GetUrlParameterReplacementValues();

            // load the OpenApi specification for every Api
            foreach (var apiUri in apiUris)
            {
                // load the OpenApi specification from Azure Resource Management
                JObject openApiSpecification = LoadOpenApiSpecification(apiUri);

                // get valid subscription for use with the Azure Api - this needs to be added to every request
                var subscriptionKeyParameterKey = openApiSpecification.Root.SelectToken("$.subscriptionKeyParameterNames.header").Value<string>();
                string subscriptionKeyParameterValue = GetSubscriptionKeyValue();

                // construct the base url that will be used for all api endpoint tests (do not need to support header or query string right now)
                string serviceUrl = openApiSpecification.Root.SelectToken("$.serviceUrl").Value<string>();
                string path = openApiSpecification.Root.SelectToken("$.path").Value<string>();
                string baseUrl = Utilities.UrlCombine(serviceUrl, path);

                // get the api version (it is okay for now to assume that it will be placed in the header)
                var apiVersionValue = openApiSpecification.Root.SelectToken("$.apiVersion").Value<string>();
                var versionHeaderScheme = openApiSpecification.Root.SelectToken("$.apiVersionSet.versionHeaderName").Value<string>();
                var apiVersion = new KeyValuePair<string, string>(versionHeaderScheme, apiVersionValue);
                // get list of api operations
                var operations = openApiSpecification.Root.SelectTokens("$.operations.value");

                // schemas contain definitions for all operations (use these to compare responses from service requests)
                var schemas = openApiSpecification.Root.SelectToken("$.schemas.value[0].document.definitions");

                // iterate over all operations for the api 
                foreach (var operation in operations.Children<JToken>())
                {
                    var httpMethod = operation.SelectToken("$.method").Value<string>();
                    var urlTemplate = operation.SelectToken("$.urlTemplate").Value<string>();
                    var operationName = operation.SelectToken("$.name").Value<string>();

                    var request = operation.SelectToken("$.request");

                    // if the operation contains a 401 response, this indicates that we must provide a valid JWT in order to get a positive test result for the operation (and not provide a JWT in order to get a negative test result)
                    bool isRequiresSubscriberJwtForSuccessfulResponse = (operation.SelectToken("$.responses[?(@.statusCode == 401)]") != null);

                    // construct tests based on the responses defined in each api operation
                    foreach (var response in operation.SelectTokens("$.responses").Children())
                    {
                        ApiOperationTest apiOperationTest = new ApiOperationTest(isRequiresSubscriberJwtForSuccessfulResponse, httpMethod, urlTemplate, operationName, schemas, urlParameterReplacementValues, baseUrl, subscriptionKeyParameterKey, subscriptionKeyParameterValue, subscriberJwt, request, response, apiVersion);
                        yield return new object[] { new MemberDataSerializer<ApiOperationTest>(apiOperationTest) };
                    }
                }
            }
        }
    }
}
