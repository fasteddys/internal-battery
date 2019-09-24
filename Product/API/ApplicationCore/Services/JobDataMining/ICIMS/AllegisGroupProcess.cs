using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace UpDiddyApi.ApplicationCore.Services.JobDataMining.ICIMS
{
    public class AllegisGroupProcess : BaseProcess, IJobDataMining
    {
        public AllegisGroupProcess(JobSite jobSite, ILogger logger, Guid companyGuid, IConfiguration config) : base(jobSite, logger, companyGuid, config ) { }
        
        public async Task<List<JobPage>> DiscoverJobPages(List<JobPage> existingJobPages)
        {
            // init code for http client and allegis group icims parser/client
            var http = new HttpClient();
            var client = new AllegisGroupClient(_jobSite.JobSiteId, _jobSite.Uri.ToString(), http);

            // get all job uris from search results (single-threaded)
            List<Uri> urls = client.GetAllJobUrisAsync().Result;
            _syslog.Log(LogLevel.Information, "***** AllegisGroupProcess.DiscoverJobPages discovered {jobCount} jobs, with {existingJobs} existing jobs", urls.Count(), existingJobPages.Count());

            // populate this list with both new and updated/existing jobs
            ConcurrentBag<JobPage> updatedJobPages = new ConcurrentBag<JobPage>();

            // diagnostics
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            var maxdop = new ParallelOptions { MaxDegreeOfParallelism = 10 };
            // iterate through search results looking for new jobs (new urls)
            Parallel.For(0, urls.Count(), maxdop, index =>
            {
                var url = urls[index];
                var existingJob = existingJobPages.Where(jp => jp.Uri == url && jp.JobPageStatusId <= 2).FirstOrDefault();
                // if there is no url collision with existing job then must be a new job
                if (existingJob == null)
                {
                    try
                    {
                        var jobPage = client.GetJobPageByUriAsync(url).Result;

                        // double check that identifier doesn't exist in existing jobs
                        existingJob = existingJobPages.Where(jp => jp.UniqueIdentifier == jobPage.UniqueIdentifier && jp.JobPageStatusId <= 2).FirstOrDefault();
                        // if no identifier collision and successful scrape then add new job
                        if (existingJob == null && jobPage != null)
                            updatedJobPages.Add(jobPage);

                    } catch(Exception e)
                    {
                        _syslog.Log(LogLevel.Information, $"***** AllegisGroupProcess.DiscoverJobPages encountered an exception while attempting to scrape new job; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}, uri: {url}");
                    }
                }

            });

            // review existing jobs to make sure they are still valid (multi-threaded)
            Parallel.For(0, existingJobPages.Count(), maxdop, index =>
            {
                var existingJobPage = existingJobPages[index];
                try
                {
                    var jobPage = client.GetJobPageByUriAsync(existingJobPage.Uri).Result;
                    // if scrape failed then delete the job
                    if (jobPage == null)
                    {
                        existingJobPage.JobPageStatusId = 4; // deleted
                    }
                    else
                    {
                        if (existingJobPage.RawData == jobPage.RawData)
                        {
                            existingJobPage.JobPageStatusId = 2;
                        }
                        else
                        {
                            existingJobPage.JobPageStatusId = 1;
                            existingJobPage.RawData = jobPage.RawData;
                        }
                    }
                    existingJobPage.ModifyDate = DateTime.UtcNow;
                }
                catch (Exception e)
                {
                    existingJobPage.JobPageStatusId = 3; // set error code
                    _syslog.Log(LogLevel.Information, $"***** AllegisGroupProcess.DiscoverJobPages encountered an exception while attempting to validate existing job; message: {e.Message}, stack trace: {e.StackTrace}, source: {e.Source}");
                }
                finally
                {
                    updatedJobPages.Add(existingJobPage);
                }

            });
            stopwatch.Stop();
            var elapsed = stopwatch.ElapsedMilliseconds;
            return updatedJobPages.ToList();
        }

        public JobPostingDto ProcessJobPage(JobPage jobPage)
        {
            var jobPostingDto = AllegisGroupClient.ParseRawData(jobPage.UniqueIdentifier, _companyGuid, jobPage.RawData);
            jobPostingDto.CreateGuid = Guid.Empty;
            jobPostingDto.IsDeleted = 0;
            jobPostingDto.IsAgencyJobPosting = true;
            jobPostingDto.ThirdPartyApply = true;
            jobPostingDto.ThirdPartyApplicationUrl = jobPage.Uri.ToString();
            jobPostingDto.JobStatus = (int)JobPostingStatus.Active;
            jobPostingDto.Company = new CompanyDto() { CompanyGuid = _companyGuid };
            jobPostingDto.ThirdPartyIdentifier = jobPage.UniqueIdentifier;
            jobPostingDto.PostingExpirationDateUTC = DateTime.UtcNow.AddYears(1);

            // For Allegis Group Corporate Jobs statically set recruiter for now
            jobPostingDto.Recruiter = new RecruiterDto()
            {
                FirstName = _configuration["JobScrape:AllegisCorporate:Recruiter:FirstName"],
                LastName = _configuration["JobScrape:AllegisCorporate:Recruiter:LastName"],
                Email = _configuration["JobScrape:AllegisCorporate:Recruiter:Email"]
            };

            return jobPostingDto;
        }
    }
}
