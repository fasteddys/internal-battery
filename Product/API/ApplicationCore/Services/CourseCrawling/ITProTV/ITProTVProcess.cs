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
using Newtonsoft.Json.Linq;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Services.CourseCrawling.Common;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Services.CourseCrawling.ITProTV
{
    public class ITProTVProcess : BaseCourseProcess, ICourseProcess
    {
        public ITProTVProcess(CourseSite courseSite, ILogger logger, IConfiguration configuration, ISovrenAPI sovrenAPI) : base(courseSite, logger, configuration, sovrenAPI)
        {

        }

        private HttpClientHandler GetHttpClientHandler()
        {
            return new HttpClientHandler()
            {
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12
            };
        }

        private ConcurrentBag<ItProTVCategory> Categories { get; set; } = new ConcurrentBag<ItProTVCategory>();

        public async Task<List<CoursePage>> DiscoverCoursePagesAsync(List<CoursePage> existingCoursePages)
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

            var maxdop = new ParallelOptions { MaxDegreeOfParallelism = 20 };
            // discover categories
            int counter = 0;
            Parallel.For(counter, courseUrls.Count(), maxdop, i =>
            {
                var category = DiscoverCourseCategory(new Uri(courseUrls[i].ToString()));
                if (category != null)
                    Categories.Add(category);
            });

            // discover courses          
            counter = 0;
            Parallel.For(counter, courseUrls.Count(), maxdop, i =>
            {
                var coursePage = DiscoverCoursePage(new Uri(courseUrls[i].ToString()), _sovrenApi);
                if (coursePage != null)
                    discoveredCoursePages.Add(coursePage);

            });

            List<CoursePage> iLoveEntityFramework = new List<CoursePage>();

            // ignore duplicates during discovery as courses can be listed multiple times under different categories
            var uniqueDiscoveredCourses = (from cp in discoveredCoursePages
                                           group cp by cp.UniqueIdentifier into g
                                           select g.OrderBy(a => a, new CompareByUniqueIdentifier()).First()).ToList();

            // set the course page status that should occur for each discovered page
            foreach (var discoveredCoursePage in uniqueDiscoveredCourses)
            {
                var existingCoursePage = existingCoursePages.Where(ecp => ecp.UniqueIdentifier == discoveredCoursePage.UniqueIdentifier).FirstOrDefault();
                if (existingCoursePage != null)
                {
                    // existing course pages with modified content should be processed
                    if (existingCoursePage.RawData != discoveredCoursePage.RawData)
                    {
                        existingCoursePage.RawData = discoveredCoursePage.RawData;
                        if (existingCoursePage.CourseId.HasValue)
                        {
                            existingCoursePage.CoursePageStatusId = 2; // update
                            existingCoursePage.CoursePageStatus = null;
                        }
                        else
                        {
                            existingCoursePage.CoursePageStatusId = 3; //create
                            existingCoursePage.CoursePageStatus = null;
                        }

                        existingCoursePage.ModifyDate = DateTime.UtcNow;
                        existingCoursePage.ModifyGuid = Guid.Empty;
                        iLoveEntityFramework.Add(existingCoursePage);
                    }
                }
                else
                {
                    discoveredCoursePage.CoursePageStatusId = 3; // create
                    iLoveEntityFramework.Add(discoveredCoursePage);
                }
            }

            // set the course page status to delete for those course pages that were not discovered
            var coursePagesToDelete = existingCoursePages.Except(uniqueDiscoveredCourses, new EqualityComparerByUniqueIdentifier()).ToList();
            foreach (var coursePageToDelete in coursePagesToDelete)
            {
                coursePageToDelete.CoursePageStatusId = 4; // delete
                coursePageToDelete.CoursePageStatus = null;
                coursePageToDelete.ModifyDate = DateTime.UtcNow;
                coursePageToDelete.ModifyGuid = Guid.Empty;
                coursePageToDelete.IsDeleted = 1;
                iLoveEntityFramework.Add(coursePageToDelete);
            }

            return iLoveEntityFramework;
        }

        public class CompareByUniqueIdentifier : IComparer<CoursePage>
        {
            public int Compare(CoursePage x, CoursePage y)
            {
                return string.Compare(x.UniqueIdentifier, y.UniqueIdentifier);
            }
        }

        public class EqualityComparerByUniqueIdentifier : IEqualityComparer<CoursePage>
        {
            public bool Equals(CoursePage x, CoursePage y)
            {
                if (Object.ReferenceEquals(x, y))
                    return true;
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;
                return x.UniqueIdentifier == y.UniqueIdentifier;
            }

            public int GetHashCode(CoursePage coursePage)
            {
                if (Object.ReferenceEquals(coursePage, null))
                    return 0;
                return coursePage.UniqueIdentifier == null ? 0 : coursePage.UniqueIdentifier.GetHashCode();
            }
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
                var abbreviation = courseCategory.DocumentNode.SelectSingleNode("//section[contains(@class,'--Hero--')]/section/div/div/h1");
                var description = courseCategory.DocumentNode.SelectSingleNode("//section[contains(@class,'--Hero--')]/section/div/div/p");
                var courseNames = courseCategory.DocumentNode.SelectNodes("//section[contains(@class, '--Courses--')]/div/div/ul/a/li/h5").Select(n => n.InnerText).ToList();

                var topic = string.Empty; // todo: hard-code values for now
                return new ItProTVCategory(abbreviation?.InnerText, description?.InnerText, courseNames);
            }
            else
            {
                return null;
            }
        }

        private CoursePage DiscoverCoursePage(Uri coursePageUri, ISovrenAPI sovrenApi)
        {
            // course pages always have 3 url segments
            if (coursePageUri.Segments.Where(s => s.Length > 1).Count() == 3)
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
                var categories = Categories.Where(c => c.CourseNames.Contains(title?.InnerText)).ToList();
                var course = new ITProTVCourse(title?.InnerText, subtitle?.InnerText, description?.InnerText, duration?.InnerText, overview?.InnerText, categories, sovrenApi);

                var coursePage = new CoursePage()
                {
                    CoursePageGuid = Guid.NewGuid(),
                    CourseSiteId = _courseSite.CourseSiteId,
                    CreateDate = DateTime.UtcNow,
                    CreateGuid = Guid.Empty,
                    IsDeleted = 0,
                    RawData = JsonConvert.SerializeObject(course),
                    UniqueIdentifier = coursePageUri.Segments.Last(),
                    Uri = coursePageUri
                };

                return coursePage;
            }
            else
            {
                return null;
            }
        }

        public async Task<CourseDto> ProcessCoursePageAsync(CoursePage coursePage)
        {
            if (coursePage.CoursePageStatus.Name == "Delete")
            {
                if (!coursePage.CourseId.HasValue)
                    return null;

                return new CourseDto() { IsDeleted = 1, CourseGuid = coursePage.Course.CourseGuid };
            }
            else
            {
                JObject rawData = JsonConvert.DeserializeObject<JObject>(coursePage.RawData);

                CourseDto courseDto = new CourseDto()
                {
                    Code = coursePage.UniqueIdentifier,
                    CreateDate = coursePage.CreateDate,
                    CreateGuid = Guid.Empty,
                    ModifyDate = coursePage.ModifyDate,
                    ModifyGuid = Guid.Empty,
                    Description = rawData["Description"].Value<string>(),
                    IsDeleted = 0,
                    Name = rawData["Title"].Value<string>(),
                    CourseVariants = new List<CourseVariantDto>(),
                    Skills = new List<SkillDto>(),
                    TagTopics = new List<TagTopicDto>(),
                    IsExternal = true
                };

                if (coursePage.CourseId.HasValue)
                    courseDto.CourseId = coursePage.CourseId.Value;

                CourseVariantTypeDto courseVariantTypeDto = new CourseVariantTypeDto() { Name = "Self-Paced" };
                CourseVariantDto courseVariantDto = new CourseVariantDto() { Price = 0, CourseVariantType = courseVariantTypeDto };
                courseDto.CourseVariants.Add(courseVariantDto);

                VendorDto vendorDto = new VendorDto() { Name = "ITPro.TV" };
                courseDto.Vendor = vendorDto;

                JArray skills = (JArray)rawData["Skills"];
                foreach (var skill in skills)
                {
                    courseDto.Skills.Add(new SkillDto() { SkillName = skill.Value<string>() });
                }

                JArray categories = (JArray)rawData["Categories"];
                foreach (var category in categories)
                {
                    string abbreviation = category["Abbreviation"].Value<string>();
                    string description = category["Description"].Value<string>();
                    TagDto tagDto = new TagDto() { Name = abbreviation, Description = description };

                    string topic = category["Topic"].Value<string>();
                    TopicDto topicDto = new TopicDto() { Name = topic };
                    TagTopicDto tagTopicDto = new TagTopicDto() { Tag = tagDto, Topic = topicDto };
                    courseDto.TagTopics.Add(tagTopicDto);
                }

                return courseDto;
            }
        }
    }
}
