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
using System.Security.Authentication;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using static UpDiddyApi.ApplicationCore.Services.JobDataMining.Helpers;

namespace UpDiddyApi.ApplicationCore.Services.JobDataMining
{
    public class EASiProcess : BaseProcess, IJobDataMining
    {
        private readonly List<EmploymentTypeDto> _employmentTypes;

        public EASiProcess(JobSite jobSite, ILogger logger, Guid companyGuid, IConfiguration config, IEmploymentTypeService employmentTypeService)
            : base(jobSite, logger, companyGuid, config, employmentTypeService)
        {
            var newEmploymentTypes = employmentTypeService.GetEmploymentTypes().Result;
            _employmentTypes = newEmploymentTypes.EmploymentTypes.Select(et => new EmploymentTypeDto() { EmploymentTypeGuid = et.EmploymentTypeGuid, Name = et.Name }).ToList();
            this._client.DefaultRequestHeaders.UserAgent.ParseAdd(@"Mozilla/5.0 (compatible; bingbot/2.0; +http://www.bing.com/bingbot.htm)");
        }

        #region Private Members

        private HttpClient _client = new HttpClient(new HttpClientHandler()
        {
            SslProtocols = SslProtocols.Tls12
        });

        private async Task<JobPage> CreateJobPageFromHttpRequest(Uri jobPageUri, List<JobPage> existingJobPages)
        {
            JobPage result = null;
            string rawHtml;
            JObject rawData;
            UriBuilder uriBuilder = new UriBuilder(jobPageUri);
            HtmlDocument jobHtml = new HtmlDocument();
            JobPage existingJobPage = null;

            try
            {
                int jobPageStatusId = 1; // pending
                bool isJobExists = true;
                // retrieve the latest job page data
                var response = await _client.GetAsync(uriBuilder.Uri);
                if (response.StatusCode != HttpStatusCode.OK)
                    isJobExists = false;
                rawHtml = await response.Content.ReadAsStringAsync();

                // all job page data is embedded in javascript and bound to the DOM by the client. as such, it makes it impossible to parse the html
                // and extract the job data. the below code is hacky as hell but it works. a slightly better way to do this would be to use the 
                // Jurassic library to execute the javascript code and get the json data that way. started down that path but got stuck - come back
                // and revisit that approach if this code becomes too fragile: https://html-agility-pack.net/knowledge-base/18156795/parsing-html-to-get-script-variable-value
                string matchStart = "phApp.ddo =";
                string matchEnd = "};";
                int indexStart = rawHtml.IndexOf(matchStart) + matchStart.Length;
                int indexEnd = rawHtml.IndexOf(matchEnd, indexStart) + 1; // not using length of the indexEnd here; want to ignore the semi-colon
                string rawJson = rawHtml.Substring(indexStart, indexEnd - indexStart);
                JObject parsedJson = JObject.Parse(rawJson);
                JToken jobData = parsedJson["jobDetail"]["data"]["job"];

                // check for an existing job page based on the RWS identifier
                existingJobPage = existingJobPages.Where(jp => jp.UniqueIdentifier == jobData["jobId"].ToString()).FirstOrDefault();
                if (existingJobPage != null)
                {
                    // check to see if the page content has changed since we last ran this process
                    if (existingJobPage.RawData == jobData.ToString())
                        jobPageStatusId = 2; // active (no action required)

                    // use the existing job page
                    existingJobPage.JobPageStatusId = jobPageStatusId;
                    existingJobPage.Uri = jobPageUri;
                    existingJobPage.RawData = jobData.ToString();
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
                        RawData = jobData.ToString(),
                        UniqueIdentifier = jobData["jobId"].ToString(),
                        Uri = jobPageUri,
                        JobSiteId = _jobSite.JobSiteId
                    };
                }

            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Information, $"***** EASiProcess.CreateJobPageFromHttpRequest encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
            }
            finally
            {
                rawHtml = null;
                rawData = null;
                uriBuilder = null;
                jobHtml = null;
                existingJobPage = null;
                GC.Collect(2, GCCollectionMode.Forced);
            }

            return result;
        }

        #endregion

        #region Public Members

        public async Task<List<JobPage>> DiscoverJobPages(List<JobPage> existingJobPages)
        {
            // populate this collection with the results of the job discovery operation
            ConcurrentBag<JobPage> discoveredJobPages = new ConcurrentBag<JobPage>();

            // diagnostics - remove this once we have tuned the process
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            // the new structure of search results page prevents us from extracting job urls. cannot figure out a way to 
            // control pagination via query string parameters or headers or the "sliderRadius" (geo distance). using the
            // sitemap for EASi instead (replaced dbo.Jobsite.Uri with https://jobs.easi.com/sitemap.xml)
            List<Uri> sitemapJobPageUrls = new List<Uri>();

            // load the sitemap as xml
            var sitemapResult = await _client.GetAsync(_jobSite.Uri);
            string sitemapAsString = await sitemapResult.Content.ReadAsStringAsync();
            XmlDocument sitemapXml = new XmlDocument();
            sitemapXml.LoadXml(sitemapAsString);

            // add the namespace used for sitemaps
            XmlNamespaceManager manager = new XmlNamespaceManager(sitemapXml.NameTable);
            manager.AddNamespace("s", @"http://www.sitemaps.org/schemas/sitemap/0.9");

            // this regex matches specific job pages (those that contain "job" followed by a number)
            Regex specificJobUrl = new Regex(@"/job/[0-9]+/");

            // identify the urls that contain specific jobs 
            XmlNodeList listOfUrls = sitemapXml.SelectNodes("/s:urlset/s:url", manager);
            foreach (XmlNode url in listOfUrls)
            {
                var locText = url["loc"].InnerText;
                if (specificJobUrl.IsMatch(locText))
                {
                    sitemapJobPageUrls.Add(new Uri(locText));
                }
            }

            // removed parallel processing with crawl delay since all job pages get parsed from the sitemap now
            foreach (Uri jobUri in sitemapJobPageUrls)
            {
                JobPage discoveredJobPage = await CreateJobPageFromHttpRequest(jobUri, existingJobPages);
                if (discoveredJobPage != null)
                    discoveredJobPages.Add(discoveredJobPage);
            }

            if (discoveredJobPages.Count() != sitemapJobPageUrls.Count)
                _syslog.Log(LogLevel.Information, $"***** EASiProcess.DiscoverJobPages found {discoveredJobPages.Count()} jobs but EASi's website indicates there should be {sitemapJobPageUrls.Count} jobs.");

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
            unreferencedActiveJobs = unreferencedActiveJobs.Select(j => { j.JobPageStatusId = 4; return j; }).ToList();

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
                jobPostingDto.Title = Helpers.ConvertJValueToString(jobData.title);
                jobPostingDto.Description = Helpers.ConvertJValueToString(jobData.structureData.description);
                string rawDatePosted = Helpers.ConvertJValueToString((jobData.postedDate));
                DateTime datePosted;
                if (DateTime.TryParse(rawDatePosted, out datePosted))
                {
                    jobPostingDto.CreateDate = datePosted;
                }
                else
                {
                    if (jobPage.JobPostingId.HasValue)
                        jobPostingDto.CreateDate = jobPage.CreateDate;
                    else
                        jobPostingDto.CreateDate = DateTime.UtcNow;
                }
                jobPostingDto.City = Helpers.ConvertJValueToString(jobData.structureData.jobLocation.address.addressLocality);
                jobPostingDto.Province = Helpers.ConvertJValueToString(jobData.structureData.jobLocation.address.addressRegion);
                jobPostingDto.Country = Helpers.ConvertJValueToString(jobData.structureData.jobLocation.address.addressCountry);

                string recruiterName = Helpers.ConvertJValueToString(jobData.recruiterName);
                string[] splitRecruiterName = recruiterName.Split(" ");
                string firstName = null, lastName = null;
                if (splitRecruiterName.Length == 2)
                {
                    firstName = splitRecruiterName[0];
                    lastName = splitRecruiterName[1];
                }
                jobPostingDto.Recruiter = new RecruiterDto()
                {
                    Email = Helpers.ConvertJValueToString(jobData.recruiterEmail),
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = Helpers.ConvertJValueToString(jobData.recruiterPhone)
                };
                string[] skills = Helpers.ConvertJArrayToStringArray(jobData.ml_skills);
                if (skills != null)
                {
                    List<SkillDto> skillsDto = new List<SkillDto>();
                    foreach (var skill in skills)
                    {
                        skillsDto.Add(new SkillDto() { SkillName = skill.Trim() });
                    }
                    List<SkillDto> distinctSkillsDto = skillsDto.Distinct().ToList();
                    if (distinctSkillsDto.Count() > 0)
                        jobPostingDto.JobPostingSkills = distinctSkillsDto;
                }
                if (_employmentTypes != null)
                {
                    string rawEmploymentType = Helpers.ConvertJValueToString(jobData.structureData.employmentType);

                    switch (rawEmploymentType)
                    {
                        case "CONTRACTOR":
                            jobPostingDto.EmploymentType = _employmentTypes.Where(et => et.Name == "Contractor").FirstOrDefault();
                            break;
                        case "FULL_TIME":
                            jobPostingDto.EmploymentType = _employmentTypes.Where(et => et.Name == "Full-Time").FirstOrDefault();
                            break;
                        case "PART_TIME":
                            jobPostingDto.EmploymentType = _employmentTypes.Where(et => et.Name == "Part-Time").FirstOrDefault();
                            break;
                        default:
                            jobPostingDto.EmploymentType = _employmentTypes.Where(et => et.Name == "Other").FirstOrDefault();
                            break;
                    }
                }

                return jobPostingDto;
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Information, $"***** EASiProcess.ProcessJobPage encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
                return null;
            }
        }

        #endregion
    }
}
