using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using static UpDiddyApi.ApplicationCore.Services.JobDataMining.Helpers;

namespace UpDiddyApi.ApplicationCore.Services.JobDataMining
{
    public class TEKsystemsProcess : BaseProcess, IJobDataMining
    {
        public TEKsystemsProcess(JobSite jobSite, ILogger logger, Guid companyGuid, IConfiguration config) : base(jobSite, logger, companyGuid, config) { }

        private HttpClientHandler GetHttpClientHandler()
        {
            return new HttpClientHandler()
            {
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12
            };
        }


        public List<JobPage> DiscoverJobPages(List<JobPage> existingJobPages)
        {
            throw new NotImplementedException();
        }
        private JobPage DiscoverJobPage(Uri jobPageUri, List<JobPage> existingJobPages)
        {
            int jobPageStatusId = 1; // pending
            string rawHtml;
            JObject rawData;
            Uri jobDetailUri = null;

            bool isJobExists = true;
            // retrieve the latest job page data
            using (var client = new HttpClient(GetHttpClientHandler()))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = jobPageUri,
                    Method = HttpMethod.Get
                };
                var result = client.SendAsync(request).Result;
                if (result.StatusCode != HttpStatusCode.OK)
                    isJobExists = false;
                rawHtml = result.Content.ReadAsStringAsync().Result;
            }
            HtmlDocument jobHtml = new HtmlDocument();
            jobHtml.LoadHtml(rawHtml);

            // check for a message indicating that the job does not exist
            if (jobHtml.DocumentNode.SelectSingleNode("//div[contains(@class, 'missing-job-bar')]") != null)
            {
                return null;
            }
            else
            {
                var jsonJobData = jobHtml.DocumentNode.SelectSingleNode("//script[@type='application/ld+json']");
                rawData = JObject.Parse(jsonJobData.InnerText);

                // add formatted job description to job data
                var jobSummaryH2 = jobHtml.DocumentNode.SelectSingleNode("//div[contains(@class, 'jdp-job-description-card')]/h2[contains(@class, 'content-card-header')]");
                jobSummaryH2.Remove();
                var descriptionFromHtml = jobHtml.DocumentNode.SelectSingleNode("//div[contains(@class, 'jdp-job-description-card')]");
                if (descriptionFromHtml != null && descriptionFromHtml.InnerHtml != null)
                    rawData.Add("formattedDescription", descriptionFromHtml.InnerHtml.Trim());

                // add the RWS identifier to job data
                var rwsId = jobHtml.DocumentNode.SelectSingleNode("//div/strong[text()='Posting ID:']/following-sibling::div");
                rawData.Add("rwsId", rwsId.InnerText.Trim());

                // retrieve recruiter information 
                var recruiterName = jobHtml.DocumentNode.SelectSingleNode("//div[contains(.,'Name:')]/following-sibling::div");
                string[] split = recruiterName.InnerText.Trim().Split(' ');
                string recruiterfirstName = null, recruiterlastName = null;
                if (split != null && split.Length >= 2)
                {
                    recruiterfirstName = split[0];
                    recruiterlastName = split[1];
                }
                var recruiterPhone = jobHtml.DocumentNode.SelectSingleNode("//div[contains(.,'Phone:')]/following-sibling::div");
                var recruiterEmail = jobHtml.DocumentNode.SelectSingleNode("//div[contains(.,'Email:')]/following-sibling::div/a");

                // add recruiter information to job data
                rawData.Add(
                    new JProperty("recruiter",
                        new JObject(
                            new JProperty("firstName", recruiterfirstName),
                            new JProperty("lastName", recruiterlastName),
                            new JProperty("phone", recruiterPhone.InnerText.Trim()),
                            new JProperty("email", recruiterEmail.InnerText.Trim()))));

                // check for an existing job page based on the RWS identifier
                var existingJobPage = existingJobPages.Where(jp => jp.UniqueIdentifier == rwsId.InnerText.Trim()).FirstOrDefault();
                if (existingJobPage != null)
                {
                    // check to see if the page content has changed since we last ran this process
                    if (existingJobPage.RawData == rawData.ToString())
                        jobPageStatusId = 2; // active (no action required)

                    existingJobPage.JobPageStatusId = jobPageStatusId;
                    existingJobPage.RawData = rawData.ToString();
                    existingJobPage.ModifyDate = DateTime.UtcNow;
                    existingJobPage.ModifyGuid = Guid.Empty;
                    return existingJobPage;
                }
                else
                {
                    // add the new job page to the collection
                    return new JobPage()
                    {
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        IsDeleted = 0,
                        JobPageGuid = Guid.NewGuid(),
                        JobPageStatusId = jobPageStatusId,
                        RawData = rawData.ToString(),
                        UniqueIdentifier = rwsId.InnerText.Trim(),
                        Uri = jobDetailUri,
                        JobSiteId = _jobSite.JobSiteId
                    };
                }
            }
        }

        public async Task<List<JobPage>> DiscoverJobPagesAsync(List<JobPage> existingJobPages)
        {
            // populate this collection with the results of the job discovery operation
            ConcurrentBag<JobPage> discoveredJobPages = new ConcurrentBag<JobPage>();

            // diagnostics - remove this once we have tuned the process
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            List<Uri> jobPageUrls = new List<Uri>();
            bool isSearchPageContainsJobs = false; // todo: false for testing purposes only; revert to true once done with small sample size!
            int pageIndex = 0;
            do
            {
                UriBuilder searchUriBuilder = new UriBuilder(_jobSite.Uri);
                searchUriBuilder.Query = "?pagenumber=" + (pageIndex++).ToString();

                string response;
                using (var client = new HttpClient(GetHttpClientHandler()))
                {
                    var request = new HttpRequestMessage()
                    {
                        RequestUri = searchUriBuilder.Uri,
                        Method = HttpMethod.Get
                    };
                    var result = client.SendAsync(request).Result;
                    response = result.Content.ReadAsStringAsync().Result;
                }

                HtmlDocument searchResultPage = new HtmlDocument();
                searchResultPage.LoadHtml(response);
                var rawJobListData = searchResultPage.DocumentNode.SelectSingleNode("//script[@type='application/ld+json']");
                var jsonJobListData = JObject.Parse(rawJobListData.InnerText);

                var searchResultPageJobUrls = (from p in jsonJobListData["itemListElement"]
                                               select (Uri)p["url"]).ToList();

                if (searchResultPageJobUrls != null && searchResultPageJobUrls.Count() > 0)
                    jobPageUrls.AddRange(searchResultPageJobUrls);
                else
                    isSearchPageContainsJobs = false;
            } while (isSearchPageContainsJobs);

            /* run the paged requests in parallel - tested with a variety of MAXDOP settings and 50 was the sweet spot locally 
             * when developing in the office. from home, i had to limit it to 15; anything higher and i started getting SSL errors.
             * i thought this had to do with my home network, but now i am getting SSL errors in the office too beyond 15 threads.
             * i think we are being throttled by the job site? limiting this to 20; if we see SSL errors in staging/prod we may need
             * to revisit this. changing to 10 because of socket exceptions that started happening once we switched to careerbuilder.
             * use maxdop = 1 for debugging.
             */

            foreach (var jobPageUrl in jobPageUrls)
            {
                JobPage discoveredJobPage = null;
                // Task.Run(() => discoveredJobPage = DiscoverJobPage(jobPageUrl, existingJobPages));
                // await Task.Delay(TimeSpan.FromSeconds(65));

                //   Task.Delay(1000).ContinueWith(t => bar());

                // try the above logic for a better delay implementation

                discoveredJobPage = DiscoverJobPage(jobPageUrl, existingJobPages);
                if (discoveredJobPage != null)
                    discoveredJobPages.Add(discoveredJobPage);
            }

            if (discoveredJobPages.Count() != jobPageUrls.Count)
                _syslog.Log(LogLevel.Information, $"***** TEKsystemsProcess.DiscoverJobPages found {discoveredJobPages.Count()} jobs but TEKsystem's API indicates there should be {jobPageUrls.Count} jobs.");

            /* deal with duplicate job postings (or job postings that are similar enough to be considered duplicates). examples:
             * - two job listings that have the same url and id but in the raw data the "applications" property is different (id: J3Q20V76L8YK2XBR6S8)
             * - two job listings that have the same id but different urls. when looking at the website, each lists a different canonical url. 
             *      we don't want to make the same mistake as this will hurt us from an SEO perspective.
             * 
             * the goal of the code below is to eliminate these duplicates in a repeatable manner before they make it to our db (dbo.JobPage). the 
             * method used is secondary to the goal of it being repeatable (in terms of importance). what we want to avoid is the following: two jobs 
             * are essentially identical; job A and job B. if yesterday the job data mining process chose job A and ignored job B, and tomorrow it 
             * chooses job B instead of job A, the end result could be inconsistent data in our scraped job and job applications. in addition, the 
             * audit trail of what we did could be quite confusing.
             */
            var uniqueDiscoveredJobs = (from jp in discoveredJobPages
                                        group jp by jp.UniqueIdentifier into g
                                        select g.OrderBy(a => a, new CompareByUri()).First()).ToList();

            // identify existing active jobs that were not discovered as valid and mark them for deletion
            var existingActiveJobs = existingJobPages.Where(jp => jp.JobPageStatusId == 2);
            var discoveredActiveAndPendingJobs = uniqueDiscoveredJobs.Where(jp => jp.JobPageStatusId == 1 || jp.JobPageStatusId == 2);
            var unreferencedActiveJobs = existingActiveJobs.Except(discoveredActiveAndPendingJobs, new EqualityComparerByUniqueIdentifier());

            // if the page didnt appear in the search results, flag it for deletion 
            unreferencedActiveJobs.Select(j => { j.JobPageStatusId = 4; return j; }).ToList();

            // combine new/modified jobs and unreferenced jobs which should be deleted
            List<JobPage> updatedJobPages = new List<JobPage>();
            updatedJobPages.AddRange(uniqueDiscoveredJobs);
            updatedJobPages.AddRange(unreferencedActiveJobs);

            // diagnostics - remove this once we have tuned the process
            stopwatch.Stop();
            var elapsed = stopwatch.ElapsedMilliseconds;

            return updatedJobPages;
        }

        public JobPostingDto ProcessJobPage(JobPage jobPage)
        {
            JobPostingDto jobPostingDto = new JobPostingDto();
            try
            {
                // set everything we can without relying on the raw data from the job page
                jobPostingDto.CreateGuid = Guid.Empty;
                jobPostingDto.IsDeleted = 0;
                jobPostingDto.IsAgencyJobPosting = true;
                jobPostingDto.ThirdPartyApplicationUrl = jobPage.Uri.ToString();
                jobPostingDto.ThirdPartyApply = true;
                jobPostingDto.JobStatus = (int)JobPostingStatus.Active;
                jobPostingDto.Company = new CompanyDto() { CompanyGuid = _companyGuid };
                jobPostingDto.ThirdPartyIdentifier = jobPage.UniqueIdentifier;
                jobPostingDto.PostingExpirationDateUTC = DateTime.UtcNow.AddYears(1);

                // everything else relies upon valid raw data
                if (!string.IsNullOrWhiteSpace(jobPage.RawData))
                {
                    var jobData = JsonConvert.DeserializeObject<dynamic>(jobPage.RawData);
                    jobPostingDto.Description = jobData.responsibilities;
                    jobPostingDto.City = jobData.city;
                    DateTime datePosted;
                    if (DateTime.TryParse(jobData.date_posted.ToString(), out datePosted))
                        jobPostingDto.CreateDate = datePosted;
                    else
                        jobPostingDto.CreateDate = DateTime.UtcNow;
                    jobPostingDto.Title = jobData.job_title;
                    jobPostingDto.Province = jobData.admin_area_1;
                    jobPostingDto.Country = jobData.country_code;
                    jobPostingDto.Country = jobPostingDto.Country.ToUpper();
                    string recruiterName = jobData.discrete_field_3;
                    string recruiterPhone = jobData.discrete_field_5;
                    Regex regex = new Regex(@"(\w+)\s?(((\w+\s?(^|\s+)[^@]+(\s+|$)))|(\w+))?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    var match = regex.Match(recruiterName);
                    string recruiterFirstName = string.IsNullOrEmpty(match.Groups[1].Value) ? null : match.Groups[1].Value;
                    string recruiterLastName = string.IsNullOrEmpty(match.Groups[2].Value) ? null : match.Groups[2].Value;
                    jobPostingDto.Recruiter = new RecruiterDto()
                    {
                        Email = jobData.discrete_field_4,
                        FirstName = recruiterFirstName,
                        LastName = recruiterLastName,
                        PhoneNumber = recruiterPhone
                    };
                    if (jobData.skills != null)
                    {
                        List<SkillDto> skillsDto = new List<SkillDto>();
                        foreach (var skill in jobData.skills)
                        {
                            if (skill != null && !string.IsNullOrWhiteSpace(skill.Value))
                                skillsDto.Add(new SkillDto() { SkillName = skill.Value });
                        }
                        if (skillsDto.Count() > 0)
                            jobPostingDto.JobPostingSkills = skillsDto;
                    }
                }

                return jobPostingDto;
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Information, $"***** TEKsystemProcess.ProcessJobPage encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
                return null;
            }
        }
    }
}
