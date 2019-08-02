using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Services.JobDataMining.ICIMS
{
    public class AllegisGroupClient
    {
        public string _url;
        private int _jobSiteId;
        private HttpClient _client;
        public AllegisGroupClient(int jobSiteId, string jobSiteUrl, HttpClient client)
        {
            _client = client;
            _jobSiteId = jobSiteId;
            _url = jobSiteUrl;
        }

        public async Task<List<Uri>> GetAllJobUrisAsync()
        {
            List<Uri> jobUris = new List<Uri>();
            string url = _url;
            do
            {
                url = HttpUtility.HtmlDecode(url);
                // call the api to retrieve a total number of job results
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Get
                };
                var result = await _client.SendAsync(request);
                string response = await result.Content.ReadAsStringAsync();

                HtmlDocument html = new HtmlDocument();
                html.LoadHtml(response);
                // grab job urls
                var jobAnchorTags = html.DocumentNode.SelectNodes("//ul[contains(@class,'iCIMS_JobsTable')]//li[contains(@class, 'row')]//div[contains(@class, 'title')]//a");
                if(jobAnchorTags != null)
                    jobUris.AddRange(jobAnchorTags.Select(s => new Uri(s.Attributes["href"]?.Value)));
                url = html.DocumentNode.SelectSingleNode("//a[contains(@class, 'glyph') and not(contains(@class,'invisible'))]//span[contains(@class, 'halflings-menu-right')]/parent::a")?.Attributes["href"]?.Value;
            } while (url != null);
            jobUris = jobUris.Where(s => s != null).ToList();

            return jobUris;
        }

        public async Task<JobPage> GetJobPageByUriAsync(Uri uri)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = uri,
                Method = HttpMethod.Get
            };
            var result = await _client.SendAsync(request);

            if (!result.IsSuccessStatusCode)
                return null;

            string response = await result.Content.ReadAsStringAsync();
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(response);

            var id = html.DocumentNode.SelectSingleNode("//dl//dt[contains(@class, 'iCIMS_JobHeaderField') and text()='Job ID']/following-sibling::dd//span")?.InnerText?.Trim();

            return new JobPage()
            {
                CreateDate = DateTime.UtcNow,
                CreateGuid = Guid.Empty,
                IsDeleted = 0,
                JobPageGuid = Guid.NewGuid(),
                JobPageStatusId = 1, // pending
                RawData = html.Text,
                UniqueIdentifier = id,
                Uri = uri,
                JobSiteId = _jobSiteId
            };
        }

        public static JobPostingDto ParseRawData(string identifier, Guid companyGuid, string rawData)
        {
            JobPostingDto jobPostingDto = new JobPostingDto()
            {
                ThirdPartyIdentifier = identifier,
                Company = new CompanyDto() { CompanyGuid = companyGuid }
            };

            if (string.IsNullOrWhiteSpace(rawData))
                return jobPostingDto;

            var data = JsonConvert.DeserializeObject<dynamic>(rawData.ToString());

            jobPostingDto.Title = data.title;
            jobPostingDto.Description = data.description;
            jobPostingDto.Country = data.jobLocation[0]?.address?.addressCountry;
            jobPostingDto.City = data.jobLocation[0]?.address?.addressLocality;
            jobPostingDto.Province = data.jobLocation[0]?.address?.addressRegion;

            return jobPostingDto;
        }
    }
}
