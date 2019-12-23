using Newtonsoft.Json.Linq;
using System.IO;
using System.Reflection;
using Xunit;

namespace API.Tests
{
    public class AzureAPITests
    {
        [Fact]
        public void ValidateAzureAPIEndpoints()
        {
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
            var protocol = openApiContent.Root.SelectToken("$.protocols");
            var path = openApiContent.Root.SelectToken("$.path");
            var subscriptionKeyParameterNames = openApiContent.Root.SelectToken("$.path"); // don't think we need this

            // assume versioning scheme will be segment (do not need to support header or query string)
            var operations = openApiContent.Root.SelectTokens("$.operations.value");
            var schemas = openApiContent.Root.SelectTokens("$.schemas.value");

            foreach(var operation in operations.Children<JToken>())
            {

            }

            // todo: test stuff
            Assert.True(true);
        }
    }
}
