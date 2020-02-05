using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Xunit;
using Xunit.Abstractions;
using API.Tests.Helpers;
using System.Text;
using System.Net.Http;
using System.Security.Authentication;
using System.Collections.Generic;
using System;

namespace API.Tests.AzureApi
{
    public class DataDrivenApiEndpointTests
    {
        private readonly ITestOutputHelper _output;

        public DataDrivenApiEndpointTests(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Theory]
        [MemberData(nameof(AzureApiDataProvider.ExtractApiOperationsFromAllApis), MemberType = typeof(AzureApiDataProvider))]
        public void Validate_All_Api_Endpoints_Conform_To_Specifications(MemberDataSerializer<ApiOperationTest> apiOperationTest)
        {

            // set up variables that will be used for the assertion
            bool isActualStatusCodeMatchesExpectedStatusCode = false;
            bool isResponseBodyMatchesResponseSchema = false;

            // emit some basic information about the test
            _output.WriteLine("********************** BEGIN TEST **********************");
            _output.WriteLine($"Operation Name: {apiOperationTest.Object.Name}");
            _output.WriteLine($"Api Version: {apiOperationTest.Object.ApiVersion}");
            _output.WriteLine($"Http Verb : {apiOperationTest.Object.HttpMethod.Method}");
            _output.WriteLine($"Expected Status Code: {apiOperationTest.Object.ExpectedStatusCode}");

            if (apiOperationTest.Object.DefinitionErrors.Count == 0)
            {
                // only execute the test if there were no definition errors
                HttpClient client = new HttpClient(new HttpClientHandler() { SslProtocols = SslProtocols.Tls12 });
                var request = new HttpRequestMessage
                {
                    RequestUri = apiOperationTest.Object.Uri,
                    Method = apiOperationTest.Object.HttpMethod,
                };
                foreach (var header in apiOperationTest.Object.Headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
                if (!string.IsNullOrWhiteSpace(apiOperationTest.Object.RequestBody))
                {
                    request.Content = new StringContent(apiOperationTest.Object.RequestBody, Encoding.UTF8, "application/json");
                }

                var response = client.SendAsync(request).Result;
                isActualStatusCodeMatchesExpectedStatusCode = (int)response.StatusCode == apiOperationTest.Object.ExpectedStatusCode;

                if (apiOperationTest.Object.ResponseSchema != null)
                {
                    try
                    {
                        var bodyContent = response.Content.ReadAsAsync<JObject>().Result;
                        isResponseBodyMatchesResponseSchema = bodyContent.IsValid(apiOperationTest.Object.ResponseSchema);
                    }
                    catch (Exception e)
                    {
                        apiOperationTest.Object.DefinitionErrors.Add($"An error occurred while trying to validate the response: {e.Message}");
                    }
                }
                else
                    isResponseBodyMatchesResponseSchema = true;

                // todo: perform cleanup for create, update, and delete operations (using TargetedObjectGuid)
                //if (apiOperationTest.Object..HasValue)
                //{
                //}
                // need separate logic for create - how to isolate the object created - parse response...

            }
            else
            {
                // output any definition errors that exist
                _output.WriteLine("Api operation was not invoked due to definition errors; see below for details:");
                foreach (var definitionError in apiOperationTest.Object.DefinitionErrors)
                {
                    _output.WriteLine($"Definition error: {definitionError}");
                }
            }

            try
            {
                Assert.True(
                    isActualStatusCodeMatchesExpectedStatusCode
                    && isResponseBodyMatchesResponseSchema
                    && apiOperationTest.Object.DefinitionErrors.Count == 0,
                    $"isActualStatusCodeMatchesExpectedStatusCode: {isActualStatusCodeMatchesExpectedStatusCode.ToString()}, isResponseBodyMatchesResponseSchema: {isResponseBodyMatchesResponseSchema.ToString()}");
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _output.WriteLine("*********************** END TEST ***********************");
            }
        }
    }
}