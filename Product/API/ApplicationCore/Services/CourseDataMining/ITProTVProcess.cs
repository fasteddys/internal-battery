using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Services.CourseDataMining
{
    public class ITProTVProcess : BaseCourseProcess, ICourseDataMining
    {
        public ITProTVProcess(CourseSite courseSite, ILogger logger, IConfiguration configuration) : base(courseSite, logger, configuration) { }

        private HttpClientHandler GetHttpClientHandler()
        {
            return new HttpClientHandler()
            {
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12
            };
        }
        public List<CoursePage> DiscoverCoursePages(List<CoursePage> existingCoursePages)
        {
            string response;
            using (var client = new HttpClient(GetHttpClientHandler()))
            {
                // call the api to retrieve a total number of job results
                var request = new HttpRequestMessage()
                {
                    RequestUri = _courseSite.Uri,
                    Method = HttpMethod.Get
                };
                var result = client.SendAsync(request).Result;
                response = result.Content.ReadAsStringAsync().Result;
            }
            var doc = XDocument.Parse(response);
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

            // further restrict this to only be actual course urls and not categories (use uri segment)
            var courseUrls = doc.Root.Elements(ns + "url")
                .Where(url => url.Element(ns + "loc").Value.Contains("/courses/"))
                .Select(x => x.Element(ns + "loc").Value)
                .ToList();

            var maxdop = new ParallelOptions { MaxDegreeOfParallelism = 10 };
            int counter = 0;
            Parallel.For(counter, courseUrls.Count(), maxdop, i =>
            {
                // break this out into a separate method call

            });


                throw new NotImplementedException();
        }

        public CourseDto ProcessCoursePage(CoursePage coursePage)
        {
            throw new NotImplementedException();
        }
    }
}
