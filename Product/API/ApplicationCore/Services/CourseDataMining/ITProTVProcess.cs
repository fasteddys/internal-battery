using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Services.CourseDataMining
{
    public class ITProTVProcess : BaseCourseProcess, ICourseDataMining
    {
        public ITProTVProcess(CourseSite courseSite, ILogger logger, IConfiguration configuration, ISovrenAPI sovrenAPI) : base(courseSite, logger, configuration, sovrenAPI) { }

        private HttpClientHandler GetHttpClientHandler()
        {
            return new HttpClientHandler()
            {
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12
            };
        }
        public List<CoursePage> DiscoverCoursePages(List<CoursePage> existingCoursePages)
        {
            // populate this collection with the results of the job discovery operation
            ConcurrentBag<CoursePage> discoveredCoursePages = new ConcurrentBag<CoursePage>();

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

            // discover course content (including tags/topics) from all urls in the sitemap
            var maxdop = new ParallelOptions { MaxDegreeOfParallelism = 1 };
            int counter = 0;
            Parallel.For(counter, courseUrls.Count(), maxdop, i =>
            {
                var coursePage = DiscoverCoursePage(new Uri(courseUrls[i].ToString()));
            });


            throw new NotImplementedException();
        }

        private ItProTVCategory DiscoverCourseCategory(Uri courseCategoryUri)
        {
            // course categories always have 2 url segments
            if (courseCategoryUri.Segments.Where(s => s.Length > 1).Count() == 2)
            {
                // request the page content for what we believe is a valid course category
                string response;
                using (var client = new HttpClient(GetHttpClientHandler()))
                {
                    var request = new HttpRequestMessage()
                    {
                        RequestUri = courseCategoryUri,
                        Method = HttpMethod.Get
                    };
                    var result = client.SendAsync(request).Result;
                    response = result.Content.ReadAsStringAsync().Result;
                }

                // extract the course category information and load it into a class specific to ITProTV
                HtmlDocument courseCategory = new HtmlDocument();
                courseCategory.LoadHtml(response);
                var abbreviation = courseCategory.DocumentNode.SelectSingleNode("//h1*[contains(@class,'--title--')]");
                var description = courseCategory.DocumentNode.SelectSingleNode("//p*[contains(@class,'--paragraph--')]");

                return new ItProTVCategory()
                {
                    Abbreviation = "",
                    Description = "",
                    Topic = ""
                };
            }
            else
            {
                return null;
            }
        }

        private CoursePage DiscoverCoursePage(Uri coursePageUri)
        {
            // course pages always have 3 url segments
            if (coursePageUri.Segments.Where(s => s.Length > 1).Count() == 3)
            {
                // todo: need to handle categorization here
                // the topics do not have their own discoverable urls in the sitemap, these will be hard-coded (or pulled from /courses/)
                // cannot retrieve the top-level "topic" (e.g. Cateogry, Certification, Job Role) from the site's html or sitemap.xml (menu is populated by js script)

                return null;
            }
            else
            {
                // request the page content for what we believe is a valid course 
                string response;
                using (var client = new HttpClient(GetHttpClientHandler()))
                {
                    var request = new HttpRequestMessage()
                    {
                        RequestUri = coursePageUri,
                        Method = HttpMethod.Get
                    };
                    var result = client.SendAsync(request).Result;
                    response = result.Content.ReadAsStringAsync().Result;
                }

                // extract the course information and load it into a class specific to ITProTV
                HtmlDocument courseHtml = new HtmlDocument();
                courseHtml.LoadHtml(response);
                var title = courseHtml.DocumentNode.SelectSingleNode("//h1/span[@itemprop=\"name\"]");
                var subtitle = courseHtml.DocumentNode.SelectSingleNode("//*[contains(@class,'--subtitle--')]");
                var description = courseHtml.DocumentNode.SelectSingleNode("//*[contains(@class,'--seoDescription--')]");
                var duration = courseHtml.DocumentNode.SelectSingleNode("//*[contains(@class, '--time--')]");
                var overview = courseHtml.DocumentNode.SelectSingleNode("//*[contains(@class, '-module--description--')]");
                var course = new ITProTVCourse(title?.InnerText, subtitle?.InnerText, description?.InnerText, duration?.InnerText, overview?.InnerText, string.Empty);

                var coursePage = new CoursePage()
                {
                    CoursePageGuid = Guid.NewGuid(),
                    CoursePageStatusId = 1, // pending for now... what if it already exists? has content changed? how to handle admin edits since last scrape?
                    CourseSiteId = _courseSite.CourseSiteId,
                    CreateDate = DateTime.UtcNow,
                    IsDeleted = 0,
                    RawData = JsonConvert.SerializeObject(course),
                    UniqueIdentifier = coursePageUri.ToString()
                };

                return coursePage;
            }
        }

        public CourseDto ProcessCoursePage(CoursePage coursePage)
        {
            throw new NotImplementedException();
        }

        public class ItProTVCategory
        {
            public string Topic { get; set; }
            public string Abbreviation { get; set; }
            public string Description { get; set; }
        }

        public class ITProTVCourse
        {
            private List<string> ParseSkillsFromSovren()
            {
                /* create a fake resume, send to Sovren, retrieve skill names from xml response. this cannot be done until Sovren is fixed!!
                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    using (var sw = new StreamWriter(ms))
                    {
                        sw.WriteLine("Title");
                        sw.WriteLine(this.Title);
                        sw.WriteLine("Subtitle");
                        sw.WriteLine(this.Subtitle);
                        sw.WriteLine("Description");
                        sw.WriteLine(this.Description);
                        sw.WriteLine("Overview");
                        sw.WriteLine(this.Overview);
                        sw.Flush();
                        ms.Seek(0, SeekOrigin.Begin);
                        bytes = ms.ToArray();
                    }
                }
                string base64 = Convert.ToBase64String(bytes);
                var sovrenResult = _sovrenApi.SubmitResumeAsync(base64).Result;
                */

                return new List<string>() {
                    "react",
                    "simple object access protocol (soap)",
                    "front end (software engineering)",
                    "cascading style sheets (css)",
                    "hypertext markup language (html)",
                    "java (programming language)"
                };
            }

            public ITProTVCourse(string title, string subtitle, string description, string duration, string overview, string category)
            {
                this.Title = title;
                this.Subtitle = subtitle;
                this.Description = description;
                this.Duration = duration;
                this.Overview = overview;
                Skills = this.ParseSkillsFromSovren();
            }

            public string Title { get; }
            public string Subtitle { get; }
            public string Description { get; }
            public string Duration { get; }
            public string Overview { get; }
            public List<string> Skills { get; }
            public string Category { get; }
        }
    }
}
