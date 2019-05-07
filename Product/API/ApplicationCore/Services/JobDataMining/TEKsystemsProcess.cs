using HtmlAgilityPack;
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

        public List<JobPage> DiscoverJobPages()
        {
            ConcurrentBag<JobPage> jobPages = new ConcurrentBag<JobPage>();
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

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

            // run the paged requests in parallel - tested with a variety of MAXDOP settings and 10 was the sweet spot locally; 
            // may need to adjust this once it is running in azure
            int counter = 0;
            Parallel.For(counter, timesToRequestResultsPage, new ParallelOptions { MaxDegreeOfParallelism = 50 }, i =>
           {
               string jobData;
               using (var client = new HttpClient())
               {
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
                   string rawHtml;
                   Uri jobDetailUri = new Uri(_jobSite.Uri.GetLeftPart(System.UriPartial.Authority) + job.job_details_url);
                   using (var client = new HttpClient())
                   {
                       var request = new HttpRequestMessage()
                       {
                           RequestUri = jobDetailUri,
                           Method = HttpMethod.Get
                       };
                       var result = client.SendAsync(request).Result;
                       rawHtml = result.Content.ReadAsStringAsync().Result;
                   }
                   HtmlDocument jobHtml = new HtmlDocument();
                   jobHtml.LoadHtml(rawHtml);
                   var scripNodetWithJson = jobHtml.DocumentNode.SelectSingleNode("(//script[@type='application/ld+json'])[1]");

                   // todo: guard against missing or invalid data?
                   var jobDataJson = JsonConvert.DeserializeObject<dynamic>(scripNodetWithJson.InnerHtml.ToString());
                   job.responsibilities = jobDataJson.responsibilities.Value;

                   jobPages.Add(new JobPage()
                   {
                       JobPageStatusId = 1,
                       RawData = job.ToString(),
                       UniqueIdentifier = job.id,
                       Uri = jobDetailUri
                   });
               }
           });

            /* deal with duplicate job postings (or job postings that are similar enough to be considered duplicates). examples:
             * - two job listings that have the same url and id but in the raw data the "applications" property is different (id: J3Q20V76L8YK2XBR6S8)
             * - two job listings that have the same id but different urls. when looking at the website, each lists a different canonical url. 
             *      we don't want to make the same mistake as this will hurt us from an SEO perspective.
             * 
             * the goal of the code below is to eliminate these duplicates in a repeatable manner. the method used is secondary to the goal of it being 
             * repeatable (in terms of importance). what we want to avoid is the following: two jobs are essentially identical; job A and job B. if 
             * yesterday the job data mining process chose job A and ignored job B, and tomorrow it chooses job B instead of job A, the end result could 
             * be inconsistent data in our scraped job and job applications. in addition, the audit trail of what we did could be quite confusing.
             */
            var uniqueJobs = (from jp in jobPages
                              group jp by jp.UniqueIdentifier into g
                              select g.OrderBy(a => a, new CompareByUri()).First()).ToList();

            stopwatch.Stop();
            var elapsed = stopwatch.ElapsedMilliseconds;

            return uniqueJobs;
        }

        public JobPosting ProcessJobPage(JobPage jobPage)
        {
            throw new NotImplementedException();
        }

        public class CompareByUri : IComparer<JobPage>
        {
            public int Compare(JobPage x, JobPage y)
            {
                return string.Compare(x.Uri.ToString(), y.Uri.ToString());
            }
        }
    }
}
