using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Services.JobDataMining
{
    public class TEKsystemsProcess : BaseProcess, IJobDataMining
    {
        public TEKsystemsProcess(JobSite jobSite) : base(jobSite) { }

        public List<JobPage> GetJobPages()
        {
            ConcurrentBag<JobPage> jobPages = new ConcurrentBag<JobPage>();

            string response;
            using (var client = new HttpClient())
            {
                // todo: refactor to make each method have one responsiblity only (or as close as we can get to this)
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
             * and call this endpoint this many times to ensure we get all jobs 
             */
            int timesToRequestResultsPage = Convert.ToInt32(Math.Ceiling((double)jobCount / 10));

            // run this in parallel?
            int counter = 0;
            Parallel.For(1, timesToRequestResultsPage, i =>
            {
                string jobData;
                using (var client = new HttpClient())
                {
                    int progress = Interlocked.Increment(ref counter);
                    UriBuilder builder = new UriBuilder(_jobSite.Uri);
                    builder.Query = "?page=" + progress.ToString();
                    var request = new HttpRequestMessage()
                    {
                        RequestUri = _jobSite.Uri,
                        Method = HttpMethod.Get
                    };
                    var result = client.SendAsync(request).Result;
                    jobData = result.Content.ReadAsStringAsync().Result;
                }
                dynamic jsonResults = JsonConvert.DeserializeObject<dynamic>(jobData);
                foreach (var job in jsonResults.results)
                {
                    jobPages.Add(new JobPage()
                    {
                        JobPageStatusId = 1,
                        RawData = job.ToString()
                        // todo: set unique identifier here
                    });
                }
            });



            return jobPages.ToList();
        }

        public JobPosting ProcessJobPage(JobPage jobPage)
        {
            throw new NotImplementedException();
        }
    }
}
