using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;

namespace API.Tests.Helpers
{
    /// <summary>
    /// Contains all information for an operation within an api which is necessary in order to execute an integration test
    /// </summary>
    public class ApiOperationTest
    {
        #region Constructor 

        public ApiOperationTest(bool isRequiresSubscriberJwtForSuccessfulResponse,
            string httpMethod,
            string urlTemplate,
            string operationName,
            JToken schemas,
            Dictionary<string, string> urlParameterReplacementValues,
            string baseUrl,
            string subscriptionKeyParameterKey,
            string subscriptionKeyParameterValue,
            string subscriberJwt,
            JToken request,
            JToken response,
            KeyValuePair<string, string> apiVersion)
        {
            this.Name = $"{operationName}";

            // if the url contains parameters, set them using test values from the config file
            var replacements = urlParameterReplacementValues
                .Where(rv => urlTemplate.Contains(rv.Key))
                .Select(rv => new { Key = rv.Key, Value = rv.Value });
            if (replacements != null)
            {
                foreach(var replacement in replacements)
                {
                    urlTemplate = urlTemplate.Replace(replacement.Key, replacement.Value);
                    Guid targetedObjectGuid;
                    if (!Guid.TryParse(replacement.Value, out targetedObjectGuid))
                        this.DefinitionErrors.Add($"Unable to cast replacement value to guid; key: {replacement.Key}, value: {replacement.Value}");
                    else
                        this.TargetedObjectIds.Add(targetedObjectGuid);
                }                
            }

            this.HttpMethod = new HttpMethod(httpMethod);
            this.Uri = new Uri(Utilities.UrlCombine(baseUrl, urlTemplate));
            this.ApiVersion = apiVersion;
            this.Headers.Add(apiVersion.Key, apiVersion.Value);

            // expect a payload in the request body if the operation is a POST or a PUT
            if (this.HttpMethod.Method == "POST" || this.HttpMethod.Method == "PUT")
            {
                try
                {
                    var rawRequestBody = request.SelectToken("$.representations[0].sample");
                    if (rawRequestBody == null || string.IsNullOrWhiteSpace(rawRequestBody.Value<string>()))
                        this.DefinitionErrors.Add("No sample request payload was found for this operation.");
                    else
                    {
                        // ensure that the sample payload is valid JSON
                        var jsonRequestBody = JObject.Parse(rawRequestBody.Value<string>());

                        // add the sample to the request body
                        this.RequestBody = jsonRequestBody.ToString(Formatting.None);
                    }
                }
                catch (JsonReaderException jre)
                {
                    this.DefinitionErrors.Add($"The sample request payload was not valid JSON: {jre.Message}");
                }
                catch (Exception e)
                {
                    this.DefinitionErrors.Add($"The sample request payload was not loaded: {e.Message}");
                }
            }

            // assume that every request is protected with a subscription
            this.Headers.Add(subscriptionKeyParameterKey, subscriptionKeyParameterValue);
            if (isRequiresSubscriberJwtForSuccessfulResponse)
                this.Headers.Add("Authorization", $"Bearer {subscriberJwt}");

            // use the status code from the response definition as the expected status code for the api endpoint test
            var statusCode = response.SelectToken("$.statusCode").Value<int>();
            switch (statusCode)
            {
                case 200:
                case 201:
                case 202:
                case 204:
                    this.ExpectedStatusCode = statusCode;
                    break;
                case 401:
                    // remove JWT if it exists as this should cause a 401 response. remove definition error that requires sample payload (if one exists)
                    this.ExpectedStatusCode = statusCode;
                    this.Headers.Remove("Authorization");
                    this.DefinitionErrors.Remove("No sample request payload was found for this operation.");
                    break;
                case 400:
                    // TODO: need to think about how to implement tests to specifically test how endpoints handle malformed requests.
                    // for now, remove all occurrences of the 400 status code from operation responses
                    this.DefinitionErrors.Add("400 response status codes are not supported for automated tests; see code comments for details");
                    break;
                default:
                    this.DefinitionErrors.Add($"Unexpected response status code: {statusCode.ToString()}");
                    break;
            }

            // use the type name from the representation to retrieve the schema, load the example, ensure that the example is valid, then save the schema with the test
            try
            {
                var representation = response.SelectToken("$.representations[0]");
                if (representation != null)
                {
                    if (representation.SelectToken($".typeName") != null)
                    {
                        string typeName = representation.SelectToken("$.typeName").Value<string>();
                        string escapedTypeName = $"['{typeName}']";
                        var responseSchema = schemas.SelectToken($"$.{escapedTypeName}");
                        if (responseSchema != null)
                        {
                            JSchema jschema = JSchema.Parse(responseSchema.ToString(Formatting.None));
                            JObject example = responseSchema["example"].Value<JObject>();
                            if (!example.IsValid(jschema))
                                this.DefinitionErrors.Add("The response example is not valid according to the schema definition.");
                            else
                                this.ResponseSchema = jschema;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                this.DefinitionErrors.Add($"An error occurred while attempting to set the response schema; message: {e.Message}, stack trace: {e.StackTrace}");
            }
        }

        #endregion

        #region Properties

        public string Name { get; set; }
        public Uri Uri { get; set; }
        public HttpMethod HttpMethod { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public string RequestBody { get; set; }
        public JToken ResponseBody { get; set; }
        public JSchema ResponseSchema { get; set; }
        public int ExpectedStatusCode { get; set; }
        public int? ActualStatusCode { get; set; }
        public KeyValuePair<string, string> ApiVersion { get; set; }
        public List<string> DefinitionErrors { get; set; } = new List<string>();
        public List<string> IntegrationErrors { get; set; } = new List<string>();
        public List<Guid> TargetedObjectIds { get; private set; } = new List<Guid>();

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"{this.Name} ({this.ExpectedStatusCode})";
        }

        #endregion
    }
}
