using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace UpDiddyApi.Business.Resume
{
    public class Sovren : ISovrenAPI
    {
        // Typed client: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2#typed-clients
        private HttpClient Client { get; }

        public Sovren(HttpClient client, IConfiguration configuration)
        {
            Client = client;
            Client.BaseAddress = new Uri(configuration["Sovren:BaseUrl"]);
            Client.DefaultRequestHeaders.Add("Accept", "application/xml");
            Client.DefaultRequestHeaders.Add("Sovren-AccountId", configuration["Sovren:AccountId"]);
            Client.DefaultRequestHeaders.Add("Sovren-ServiceKey", configuration["Sovren:ServiceKey"]);
        }

        public async Task<String> SubmitResumeAsync(string base64Resume)
        {
            SovrenResume resume = new SovrenResume();
            resume.DocumentAsBase64String = base64Resume;
            var stringPayload = JsonConvert.SerializeObject(resume);
            HttpContent content = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await Client.PostAsync("parser/resume", content);

            XmlDocument sovrenXML = new XmlDocument();
            sovrenXML.Load(await response.Content.ReadAsStreamAsync());
            string xPathString = "//ParsedDocument";
            XmlNode xmlNode = sovrenXML.DocumentElement.SelectSingleNode(xPathString);
            return xmlNode.InnerText;
        }
    }

    public class SovrenResume
    {
        public string DocumentAsBase64String { get; set; }
    }
}