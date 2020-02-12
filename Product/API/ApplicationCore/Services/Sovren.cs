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
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class Sovren : ISovrenAPI
    {
        // Typed client: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2#typed-clients
        private HttpClient Client { get; }
        private IRepositoryWrapper _repositoryWrapper;

        public Sovren(HttpClient client, IConfiguration configuration, IRepositoryWrapper repositoryWrapper)
        {
            Client = client;
            Client.BaseAddress = new Uri(configuration["Sovren:BaseUrl"]);
            Client.DefaultRequestHeaders.Add("Accept", "application/xml");
            Client.DefaultRequestHeaders.Add("Sovren-AccountId", configuration["Sovren:AccountId"]);
            Client.DefaultRequestHeaders.Add("Sovren-ServiceKey", configuration["Sovren:ServiceKey"]);

            _repositoryWrapper = repositoryWrapper;

        }

        public async Task<String> SubmitResumeAsync(int subscriberId, string base64Resume)
        {

            SovrenResume resume = new SovrenResume();
            resume.DocumentAsBase64String = base64Resume;
            var stringPayload = JsonConvert.SerializeObject(resume);
            HttpContent content = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            DateTime Start = DateTime.UtcNow;
            HttpResponseMessage response = await Client.PostAsync("parser/resume", content);

            XmlDocument sovrenXML = new XmlDocument();
            var responseStream = await response.Content.ReadAsStreamAsync();
            DateTime Stop = DateTime.UtcNow;        
            sovrenXML.Load(responseStream);
            string xPathString = "//ParsedDocument";
            XmlNode xmlNode = sovrenXML.DocumentElement.SelectSingleNode(xPathString);

            // save stats 
            TimeSpan Delta = Stop - Start;
            SovrenParseStatistic stat = new SovrenParseStatistic()
            {
                NumTicks = Delta.Ticks,
                SubscriberId = subscriberId,
                ResumeText = base64Resume,
                CreateDate = DateTime.UtcNow,
                CreateGuid = Guid.Empty,
                SovrenParseStatisticsGuid = Guid.NewGuid(),
                IsDeleted = 0
            };

            await _repositoryWrapper.SovrenParseStatisticRepository.Create(stat);
            await _repositoryWrapper.SovrenParseStatisticRepository.SaveAsync();
            return xmlNode.InnerText;
        }
    }

    public class SovrenResume
    {
        public string DocumentAsBase64String { get; set; }
    }
}