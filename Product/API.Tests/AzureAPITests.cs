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
using System.Data.SqlClient;
using System.Data;

namespace API.Tests.AzureApi
{
    public class DataDrivenApiEndpointTests
    {
        private readonly ITestOutputHelper _output;
        // todo: make this data-driven and pull it from a secure location
        private readonly string CONNECTION_STRING = @"Server=tcp:careercircle.database.windows.net,1433;Initial Catalog=careercirclestaging;Persist Security Info=False;User ID=CareerCircleSa;Password=j3LswgvT!!2tR#dz@eFTaqrjEdByCKD9m63Jan%B;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private readonly string SQL_DELETE_OBJECT_BY_GUID = @"[dbo].[System_Delete_ObjectByGuid]";
        private readonly string SQL_UNDELETE_OBJECT_BY_GUID = @"[dbo].[System_Undelete_ObjectByGuid]";

        public DataDrivenApiEndpointTests(ITestOutputHelper output)
        {
            this._output = output;
        }

        private void DeleteObjectByGuid(Guid objectIdentifier)
        {
            using (var connection = new SqlConnection(CONNECTION_STRING))
            {
                var command = new SqlCommand(SQL_DELETE_OBJECT_BY_GUID, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@ObjectIdentifier", SqlDbType.UniqueIdentifier).Value = objectIdentifier;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        private void UndeleteObjectByGuid(Guid objectIdentifier)
        {
            using (var connection = new SqlConnection(CONNECTION_STRING))
            {
                var command = new SqlCommand(SQL_UNDELETE_OBJECT_BY_GUID, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("@ObjectIdentifier", SqlDbType.UniqueIdentifier).Value = objectIdentifier;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        [Theory(DisplayName = "{0}")]
        [MemberData(nameof(AzureApiDataProvider.ExtractApiOperationsFromAllApis), MemberType = typeof(AzureApiDataProvider))]
        public void Validate_All_Api_Endpoints_Conform_To_Specifications(MemberDataSerializer<ApiOperationTest> apiOperationTest)
        {
            // set up variables that will be used for the assertion
            bool isActualStatusCodeMatchesExpectedStatusCode = false;
            bool isResponseBodyMatchesResponseSchema = false;
            bool isPerformedCleanupIfNecessary = true;

            // emit some basic information about the test
            _output.WriteLine($"Operation Name: {apiOperationTest.Object.Name}");
            _output.WriteLine($"Api Version: {apiOperationTest.Object.ApiVersion}");
            _output.WriteLine($"Http Verb: {apiOperationTest.Object.HttpMethod.Method}");
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

                try
                {
                    var response = client.SendAsync(request).Result;
                    apiOperationTest.Object.ResponseBody = response.Content.ReadAsAsync<string>().Result;
                    apiOperationTest.Object.ActualStatusCode = (int)response.StatusCode;
                    isActualStatusCodeMatchesExpectedStatusCode = apiOperationTest.Object.ActualStatusCode == apiOperationTest.Object.ExpectedStatusCode;
                }
                catch (Exception e)
                {
                    apiOperationTest.Object.IntegrationErrors.Add($"An error occurred while calling the Api operation: {e.Message}");
                }

                // output information about the response received
                _output.WriteLine($"Actual Status Code: {apiOperationTest.Object.ActualStatusCode}");
                _output.WriteLine($"Response Body: {apiOperationTest.Object.ResponseBody}");

                if (apiOperationTest.Object.ResponseSchema != null)
                {
                    try
                    {
                        var bodyContent = JObject.Parse(apiOperationTest.Object.ResponseBody);
                        isResponseBodyMatchesResponseSchema = bodyContent.IsValid(apiOperationTest.Object.ResponseSchema);
                    }
                    catch (Exception e)
                    {
                        apiOperationTest.Object.IntegrationErrors.Add($"An error occurred while trying to validate the response body using the defined schema: {e.Message}");
                    }
                }
                else
                    isResponseBodyMatchesResponseSchema = true;

                // perform data cleanup if received a response from the integration test
                if (apiOperationTest.Object.ActualStatusCode.HasValue)
                {
                    switch (apiOperationTest.Object.HttpMethod)
                    {
                        case HttpMethod m when m == HttpMethod.Post && apiOperationTest.Object.ActualStatusCode == 201:
                            // physically delete the entity that was created by referencing the object guid in the http response body
                            try
                            {
                                Guid entityIdentifier = Guid.Parse(apiOperationTest.Object.ResponseBody);
                                this.DeleteObjectByGuid(entityIdentifier);
                            }
                            catch (Exception e)
                            {
                                apiOperationTest.Object.IntegrationErrors.Add($"An error occurred while attempting to delete a created entity: {e.Message}");
                                isPerformedCleanupIfNecessary = false;
                            }
                            break;
                        case HttpMethod m when m == HttpMethod.Put:
                            // do nothing (entity used for testing will always have test values with an updated modifyDate)
                            break;
                        case HttpMethod m when m == HttpMethod.Delete && apiOperationTest.Object.ActualStatusCode == 204 && apiOperationTest.Object.TargetedObjectIds.Count > 0:
                            // remove the logical delete flag from the entities that were deleted by referencing the object guids in the http request
                            try
                            {
                                // it's okay if we "undelete" multiple objects (e.g. country & state present in the url to delete an entity, it is okay to "undelete" the country and state)
                                foreach(Guid entityIdentifier in apiOperationTest.Object.TargetedObjectIds)
                                {
                                    this.UndeleteObjectByGuid(entityIdentifier);
                                }
                            }
                            catch (Exception e)
                            {
                                apiOperationTest.Object.IntegrationErrors.Add($"An error occurred while attempting to restore a deleted entity: {e.Message}");
                                isPerformedCleanupIfNecessary = false;
                            }
                            break;
                        case HttpMethod m when m == HttpMethod.Get:
                            // do nothing (get operations should never modify system data)
                            break;
                        default:
                            apiOperationTest.Object.IntegrationErrors.Add($"Unsupported combination of Request Http Method ({apiOperationTest.Object.HttpMethod.Method}) and Response Status Code ({apiOperationTest.Object.ActualStatusCode}); no clean-up was performed!");
                            isPerformedCleanupIfNecessary = false;
                            break;
                    }
                }
            }
            else
            {
                // output any definition errors that exist
                _output.WriteLine("The Api operation was not invoked due to definition errors; see below for details:");
                foreach (var definitionError in apiOperationTest.Object.DefinitionErrors)
                {
                    _output.WriteLine($"Definition error: {definitionError}");
                }
            }

            if (apiOperationTest.Object.IntegrationErrors.Count > 0)
            {
                // output any integration errors that occurred
                _output.WriteLine("The Api operation encountered the following integration errors; see below for details:");
                foreach (var integrationError in apiOperationTest.Object.IntegrationErrors)
                {
                    _output.WriteLine($"Integration error: {integrationError}");
                }
            }

            // perform assertion
            Assert.True(isActualStatusCodeMatchesExpectedStatusCode
                && isResponseBodyMatchesResponseSchema
                && isPerformedCleanupIfNecessary
                && apiOperationTest.Object.DefinitionErrors.Count == 0
                && apiOperationTest.Object.IntegrationErrors.Count == 0);
        }
    }
}