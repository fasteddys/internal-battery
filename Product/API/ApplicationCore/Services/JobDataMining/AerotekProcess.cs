using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
    public class AerotekProcess : BaseProcess, IJobDataMining
    {
        public AerotekProcess(JobSite jobSite, ILogger logger, Guid companyGuid) : base(jobSite, logger, companyGuid) { }

        private HttpClientHandler GetHttpClientHandler()
        {
            return new HttpClientHandler()
            {
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12
            };
        }

        public List<JobPage> DiscoverJobPages(List<JobPage> existingJobPages)
        {
            // populate this collection with the results of the job discovery operation
            ConcurrentBag<JobPage> discoveredJobPages = new ConcurrentBag<JobPage>();

            // diagnostics - remove this once we have tuned the process
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            string response;
            using (var client = new HttpClient(GetHttpClientHandler()))
            {
                // call the api to retrieve a total number of job results
                var request = new HttpRequestMessage()
                {
                    RequestUri = _jobSite.Uri,
                    Method = HttpMethod.Get
                };
                var result = client.SendAsync(request).Result;
                response = result.Content.ReadAsStringAsync().Result;
            }
            dynamic jsonData = JsonConvert.DeserializeObject<dynamic>(response);
            int jobCount = Convert.ToInt32(jsonData.num_found);

            /* haven't been able to determine a way to change the size of the 'results' collection 
             * using query string params, it seems to always be 10. until we can figure out a way 
             * to retrieve all job postings at once, divide the number of jobs by 10 (rounding up) 
             * and call this endpoint that many times to ensure we get all jobs 
             */
            int timesToRequestResultsPage = Convert.ToInt32(Math.Ceiling((double)jobCount / 10));

            /* run the paged requests in parallel - tested with a variety of MAXDOP settings and 50 was the sweet spot locally 
             * when developing in the office. from home, i had to limit it to 15; anything higher and i started getting SSL errors.
             * i thought this had to do with my home network, but now i am getting SSL errors in the office too beyond 15 threads.
             * i think we are being throttled by the job site? limiting this to 20; if we see SSL errors in staging/prod we may need
             * to revisit this. changing to 10 because of socket exceptions that started happening once we switched to careerbuilder.
             * use maxdop = 1 for debugging.
             */

            var maxdop = new ParallelOptions { MaxDegreeOfParallelism = 10 };
            int counter = 0;
            Parallel.For(counter, timesToRequestResultsPage, maxdop, i =>
            {
                string jobData;
                using (var client = new HttpClient(GetHttpClientHandler()))
                {
                    // call the api to retrieve a list of results incrementing the page number each time
                    int progress = Interlocked.Increment(ref counter);
                    UriBuilder builder = new UriBuilder(_jobSite.Uri);
                    builder.Query += "&page=" + progress.ToString();
                    var request = new HttpRequestMessage()
                    {
                        RequestUri = builder.Uri,
                        Method = HttpMethod.Get
                    };
                    var result = client.SendAsync(request).Result;
                    jobData = result.Content.ReadAsStringAsync().Result;
                }
                dynamic jsonResults = JsonConvert.DeserializeObject<dynamic>(jobData);

                // keeping this loop serial rather than parallel intentionally (nesting parallel loops can quickly cause performance issues)
                foreach (var job in jsonResults.results)
                {
                    int jobPageStatusId = 1; // pending
                    string rawHtml;
                    Uri jobDetailUri = null;
                    try
                    {
                        bool isJobExists = true;
                        // retrieve the latest job page data
                        jobDetailUri = new Uri(_jobSite.Uri.GetLeftPart(System.UriPartial.Authority) + job.job_details_url);
                        using (var client = new HttpClient(GetHttpClientHandler()))
                        {
                            var request = new HttpRequestMessage()
                            {
                                RequestUri = jobDetailUri,
                                Method = HttpMethod.Get
                            };
                            var result = client.SendAsync(request).Result;
                            if (result.StatusCode == HttpStatusCode.Forbidden || result.StatusCode == HttpStatusCode.NotFound)
                                isJobExists = false;
                            rawHtml = result.Content.ReadAsStringAsync().Result;
                        }
                        HtmlDocument jobHtml = new HtmlDocument();
                        jobHtml.LoadHtml(rawHtml);

                        // does the html contain an error message indicating the job does not exist?
                        if (jobHtml.DocumentNode.SelectSingleNode("//results-main[@error-message=\"The job you have requested cannot be found. Please see our complete list of jobs below.\"]") != null)
                            isJobExists = false;

                        if (!isJobExists)
                        {
                            jobPageStatusId = 4; // delete
                        }
                        else
                        {
                            // append additional data that is not present in search results for the page, status already marked as new
                            var descriptionFromHtml = jobHtml.DocumentNode.SelectSingleNode("//div[@class=\"job-description\"]");
                            if (descriptionFromHtml != null && descriptionFromHtml.InnerHtml != null)
                                job.responsibilities = descriptionFromHtml.InnerHtml.Trim();
                        }

                        // get the related JobPostingId (if one exists)
                        string jobId = job.display_job_id;
                        var existingJobPage = existingJobPages.Where(jp => jp.UniqueIdentifier == jobId).FirstOrDefault();
                        if (existingJobPage != null)
                        {
                            // check to see if the page content has changed since we last ran this process
                            if (existingJobPage.RawData == job.ToString())
                                jobPageStatusId = 2; // active (no action required)

                            existingJobPage.JobPageStatusId = jobPageStatusId;
                            existingJobPage.RawData = job.ToString();
                            existingJobPage.ModifyDate = DateTime.UtcNow;
                            existingJobPage.ModifyGuid = Guid.Empty;
                            discoveredJobPages.Add(existingJobPage);
                        }
                        else
                        {
                            // add the new job page to the collection
                            discoveredJobPages.Add(new JobPage()
                            {
                                CreateDate = DateTime.UtcNow,
                                CreateGuid = Guid.Empty,
                                IsDeleted = 0,
                                JobPageGuid = Guid.NewGuid(),
                                JobPageStatusId = jobPageStatusId,
                                RawData = job.ToString(),
                                UniqueIdentifier = jobId,
                                Uri = jobDetailUri,
                                JobSiteId = _jobSite.JobSiteId
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        jobPageStatusId = 3; // record that an error occurred while processing this job page
                        _syslog.Log(LogLevel.Information, $"***** AerotekProcess.DiscoverJobPages encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
                    }
                }
            });

            if (discoveredJobPages.Count() != jobCount)
                _syslog.Log(LogLevel.Information, $"***** AerotekProcess.DiscoverJobPages found {discoveredJobPages.Count()} jobs but Aerotek's API indicates there should be {jobCount} jobs.");

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
            // rather than eliminating jobs on the basis that they don't exist in the job site's search results, make a request to each one.
            // if the page returns 200 OK, leave it alone. if anything else occurs, mark it for deletion.
            ConcurrentBag<JobPage> jobsToDelete = new ConcurrentBag<JobPage>();
            Parallel.ForEach(unreferencedActiveJobs, maxdop, unreferencedActiveJob =>
            {
                bool isJobPageExists = false;
                try
                {
                    string rawHtml;
                    using (var client = new HttpClient(GetHttpClientHandler()))
                    {
                        // call the api to retrieve a list of results incrementing the page number each time
                        UriBuilder builder = new UriBuilder(unreferencedActiveJob.Uri);
                        var request = new HttpRequestMessage()
                        {
                            RequestUri = builder.Uri,
                            Method = HttpMethod.Get
                        };
                        var result = client.SendAsync(request).Result;
                        rawHtml = result.Content.ReadAsStringAsync().Result;
                        HtmlDocument jobHtml = new HtmlDocument();
                        jobHtml.LoadHtml(rawHtml);
                        isJobPageExists = jobHtml.DocumentNode.SelectSingleNode("//results-main[@error-message=\"The job you have requested cannot be found. Please see our complete list of jobs below.\"]") == null ? true : false;
                    }
                }
                catch (Exception e)
                {
                    _syslog.Log(LogLevel.Information, $"***** AerotekProcess.DiscoverJobPages encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
                }
                finally
                {
                    if (!isJobPageExists)
                    {
                        unreferencedActiveJob.JobPageStatusId = 4;
                        jobsToDelete.Add(unreferencedActiveJob);
                    }
                }
            });

            // combine new/modified jobs and unreferenced jobs which should be deleted
            List<JobPage> updatedJobPages = new List<JobPage>();
            updatedJobPages.AddRange(uniqueDiscoveredJobs);
            updatedJobPages.AddRange(jobsToDelete);

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
                }

                return jobPostingDto;
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Information, $"***** AerotekProcess.ProcessJobPage encountered an exception; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
                return null;
            }
        }
    }
}
