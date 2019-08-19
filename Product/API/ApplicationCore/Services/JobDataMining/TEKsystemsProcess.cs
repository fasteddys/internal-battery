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

        #region Private Members

        private HttpClientHandler GetHttpClientHandler()
        {
            return new HttpClientHandler()
            {
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12
            };
        }

        private JobPage CreateJobPageFromHttpRequest(Uri jobPageUri, List<JobPage> existingJobPages)
        {
            JobPage result = null;

            try
            {
                int jobPageStatusId = 1; // pending
                string rawHtml;
                JObject rawData;

                bool isJobExists = true;
                // retrieve the latest job page data
                using (var client = new HttpClient(GetHttpClientHandler()))
                {
                    var request = new HttpRequestMessage()
                    {
                        RequestUri = jobPageUri,
                        Method = HttpMethod.Get
                    };
                    var response = client.SendAsync(request).Result;
                    if (response.StatusCode != HttpStatusCode.OK)
                        isJobExists = false;
                    rawHtml = response.Content.ReadAsStringAsync().Result;
                }
                HtmlDocument jobHtml = new HtmlDocument();
                jobHtml.LoadHtml(rawHtml);

                // check for a message indicating that the job does not exist and continue only if we do not find one
                if (jobHtml.DocumentNode.SelectSingleNode("//div[contains(@class, 'missing-job-bar')]") == null)
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
                    if (split != null)
                    {
                        if (split.Length == 1)
                        {
                            recruiterfirstName = split[0];
                        }
                        else if (split.Length >= 2)
                        {
                            recruiterfirstName = split[0];
                            recruiterlastName = split[1];
                        }
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

                        // use the existing job page
                        existingJobPage.JobPageStatusId = jobPageStatusId;
                        existingJobPage.Uri = jobPageUri;
                        existingJobPage.RawData = rawData.ToString();
                        existingJobPage.ModifyDate = DateTime.UtcNow;
                        existingJobPage.ModifyGuid = Guid.Empty;
                        result = existingJobPage;
                    }
                    else
                    {
                        // create a new job page
                        result = new JobPage()
                        {
                            CreateDate = DateTime.UtcNow,
                            CreateGuid = Guid.Empty,
                            IsDeleted = 0,
                            JobPageGuid = Guid.NewGuid(),
                            JobPageStatusId = jobPageStatusId,
                            RawData = rawData.ToString(),
                            UniqueIdentifier = rwsId.InnerText.Trim(),
                            Uri = jobPageUri,
                            JobSiteId = _jobSite.JobSiteId
                        };
                    }
                }
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Information, $"***** TEKsystemProcess.CreateJobPageFromHttpRequest encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
            }

            return result;
        }

        #endregion

        #region Public Members

        public List<JobPage> DiscoverJobPages(List<JobPage> existingJobPages)
        {
            // populate this collection with the results of the job discovery operation
            ConcurrentBag<JobPage> discoveredJobPages = new ConcurrentBag<JobPage>();

            // diagnostics - remove this once we have tuned the process
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            // retrieve all of the individual job urls by crawling the search results
            List<Uri> jobPageUrls = new List<Uri>();
            bool isSearchPageContainsJobs = true; 
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
                {
                    jobPageUrls.AddRange(searchResultPageJobUrls);
                    Thread.Sleep(_jobSite.CrawlDelayInMilliseconds.Value);
                }
                else
                    isSearchPageContainsJobs = false;
            } while (isSearchPageContainsJobs);

            // crawl all of the job urls and create job pages for each
            (jobPageUrls.ForEachWithDelay(jobPageUri => Task.Run(() =>
            {
                JobPage discoveredJobPage = null;
                discoveredJobPage = CreateJobPageFromHttpRequest(jobPageUri, existingJobPages);
                if (discoveredJobPage != null)
                    discoveredJobPages.Add(discoveredJobPage);
            }), _jobSite.CrawlDelayInMilliseconds.Value)).Wait();

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
                var jobData = JsonConvert.DeserializeObject<dynamic>(jobPage.RawData);
                jobPostingDto.Title = (jobData.title).Value;
                jobPostingDto.Description = (jobData.formattedDescription).Value;
                jobPostingDto.CreateDate = (jobData.datePosted).Value;
                jobPostingDto.City = (jobData.jobLocation.address.addressLocality).Value;
                jobPostingDto.Province = (jobData.jobLocation.address.addressRegion).Value;
                jobPostingDto.Country = (jobData.jobLocation.address.addressCountry).Value;
                jobPostingDto.Recruiter = new RecruiterDto()
                {
                    Email = (jobData.recruiter.email).Value,
                    FirstName = (jobData.recruiter.firstName).Value,
                    LastName = (jobData.recruiter.lastName).Value,
                    PhoneNumber = (jobData.recruiter.phone).Value
                };
                if (jobData.skills != null)
                {
                    string[] skills = (jobData.skills).Value.Split(',');
                    List<SkillDto> skillsDto = new List<SkillDto>();
                    foreach (var skill in skills)
                    {
                        skillsDto.Add(new SkillDto() { SkillName = skill.Trim() });
                    }
                    if (skillsDto.Count() > 0)
                        jobPostingDto.JobPostingSkills = skillsDto;
                }

                return jobPostingDto;
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Information, $"***** TEKsystemProcess.ProcessJobPage encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
                return null;
            }
        }

        #endregion
    }
}