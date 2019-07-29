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

            var maxdop = new ParallelOptions { MaxDegreeOfParallelism = 1 };
            int counter = 0;
            Parallel.For(counter, courseUrls.Count(), maxdop, i =>
            {
                // break this out into a separate method call
                var coursePage = DiscoverCoursePage(new Uri(courseUrls[i].ToString()));
            });


            throw new NotImplementedException();
        }

        private CoursePage DiscoverCoursePage(Uri coursePageUri)
        {

            if (coursePageUri.Segments.Where(s => s.Length > 1).Count() < 3)
            {
                // todo: need to handle categorization here
                // the topics do not have their own discoverable urls in the sitemap, these will be hard-coded (or pulled from /courses/)
                // cannot retrieve the top-level "topic" (e.g. Cateogry, Certification, Job Role) from the site's html or sitemap.xml (menu is populated by js script)

                return null;
            }
            else
            {
                string response;
                using (var client = new HttpClient(GetHttpClientHandler()))
                {
                    // call the api to retrieve a total number of job results
                    var request = new HttpRequestMessage()
                    {
                        RequestUri = coursePageUri,
                        Method = HttpMethod.Get
                    };
                    var result = client.SendAsync(request).Result;
                    response = result.Content.ReadAsStringAsync().Result;
                }

                HtmlDocument courseHtml = new HtmlDocument();
                courseHtml.LoadHtml(response);

                var course = new ITProTVCourse();
                var title = courseHtml.DocumentNode.SelectSingleNode("//h1/span[@itemprop=\"name\"]");
                if (title != null && title.InnerText != null)
                    course.Title = title.InnerText;

                var subtitle = courseHtml.DocumentNode.SelectSingleNode("//*[contains(@class,'--subtitle--')]");
                if (subtitle != null && title.InnerText != null)
                    course.Subtitle = subtitle.InnerText;

                var description = courseHtml.DocumentNode.SelectSingleNode("//*[contains(@class,'--seoDescription--')]");
                if (description != null && description.InnerText != null)
                    course.Description = description.InnerText;

                var duration = courseHtml.DocumentNode.SelectSingleNode("//*[contains(@class, '--time--')]");
                if (duration != null && duration.InnerText != null)
                    course.Duration = duration.InnerText;


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

                // create base64 resume file from raw data
                using (MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(coursePage.RawData)))
                {
                    byte[] bytes = ms.ToArray();
                    string base64String = Convert.ToBase64String(bytes);
                    var test = _sovrenApi.SubmitResumeAsync(base64String).Result;

                    // OpenXML lib? https://github.com/OfficeDev/Open-XML-SDK

                    /* can get the same results writing straight to a text file...
file content:

Title
CompTIA IT Fundamentals+ (Exam FC0-U61)
Subtitle
Essential IT skills and knowledge
Description
Official CompTIA online IT training. This course covers basic IT literacy and ensures you understand the terminology and concepts involved in the IT industry.
Overview
The CompTIA IT Fundamentals Certification is an entry level certification designed to introduce users to basic computer principles. It covers basic IT literacy and ensures one understands the different terminology and the various concepts involved In the IT industry. It also serves as a great starting point if you are just getting started in computers and is designed to be the first step on your way to pursuing the CompTIA A+ certification or others similar. Topics covered include hardware basics, troubleshooting, software installation, security and also networking.

base64 encoded value:
VGl0bGUNCkNvbXBUSUEgSVQgRnVuZGFtZW50YWxzKyAoRXhhbSBGQzAtVTYxKQ0KU3VidGl0bGUNCkVzc2VudGlhbCBJVCBza2lsbHMgYW5kIGtub3dsZWRnZQ0KRGVzY3JpcHRpb24NCk9mZmljaWFsIENvbXBUSUEgb25saW5lIElUIHRyYWluaW5nLiBUaGlzIGNvdXJzZSBjb3ZlcnMgYmFzaWMgSVQgbGl0ZXJhY3kgYW5kIGVuc3VyZXMgeW91IHVuZGVyc3RhbmQgdGhlIHRlcm1pbm9sb2d5IGFuZCBjb25jZXB0cyBpbnZvbHZlZCBpbiB0aGUgSVQgaW5kdXN0cnkuDQpPdmVydmlldw0KVGhlIENvbXBUSUEgSVQgRnVuZGFtZW50YWxzIENlcnRpZmljYXRpb24gaXMgYW4gZW50cnkgbGV2ZWwgY2VydGlmaWNhdGlvbiBkZXNpZ25lZCB0byBpbnRyb2R1Y2UgdXNlcnMgdG8gYmFzaWMgY29tcHV0ZXIgcHJpbmNpcGxlcy4gSXQgY292ZXJzIGJhc2ljIElUIGxpdGVyYWN5IGFuZCBlbnN1cmVzIG9uZSB1bmRlcnN0YW5kcyB0aGUgZGlmZmVyZW50IHRlcm1pbm9sb2d5IGFuZCB0aGUgdmFyaW91cyBjb25jZXB0cyBpbnZvbHZlZCBJbiB0aGUgSVQgaW5kdXN0cnkuIEl0IGFsc28gc2VydmVzIGFzIGEgZ3JlYXQgc3RhcnRpbmcgcG9pbnQgaWYgeW91IGFyZSBqdXN0IGdldHRpbmcgc3RhcnRlZCBpbiBjb21wdXRlcnMgYW5kIGlzIGRlc2lnbmVkIHRvIGJlIHRoZSBmaXJzdCBzdGVwIG9uIHlvdXIgd2F5IHRvIHB1cnN1aW5nIHRoZSBDb21wVElBIEErIGNlcnRpZmljYXRpb24gb3Igb3RoZXJzIHNpbWlsYXIuIFRvcGljcyBjb3ZlcmVkIGluY2x1ZGUgaGFyZHdhcmUgYmFzaWNzLCB0cm91Ymxlc2hvb3RpbmcsIHNvZnR3YXJlIGluc3RhbGxhdGlvbiwgc2VjdXJpdHkgYW5kIGFsc28gbmV0d29ya2luZy4=

skill taxonomy output:
<sov:SkillsTaxonomyOutput>
	<sov:TaxonomyRoot name="Sovren">
		<sov:Taxonomy name="Information Technology" id="10" percentOfOverall="50">
			<sov:Subtaxonomy name="Privacy and Data Security" id="556" percentOfOverall="25" percentOfParentTaxonomy="50">
				<sov:Skill name="COMPTIA" id="5551297" existsInText="true" whereFound="Found in SUMMARY"></sov:Skill>
			</sov:Subtaxonomy>
			<sov:Subtaxonomy name="Operations, Monitoring and Software Management" id="349" percentOfOverall="25" percentOfParentTaxonomy="50">
				<sov:Skill name="NETWORKING" id="3490044" existsInText="true" whereFound="Found in SUMMARY"></sov:Skill>
			</sov:Subtaxonomy>
		</sov:Taxonomy>
		<sov:Taxonomy name="Administrative or Clerical" id="1" percentOfOverall="50">
			<sov:Subtaxonomy name="Admin" id="113" percentOfOverall="25" percentOfParentTaxonomy="50">
				<sov:Skill name="ENTRY LEVEL" id="081173" existsInText="true" whereFound="Found in SUMMARY"></sov:Skill>
			</sov:Subtaxonomy>
			<sov:Subtaxonomy name="Entry Level" id="499" percentOfOverall="25" percentOfParentTaxonomy="50">
				<sov:Skill name="ENTRY LEVEL" id="081158" existsInText="true" whereFound="Found in SUMMARY"></sov:Skill>
			</sov:Subtaxonomy>
		</sov:Taxonomy>
	</sov:TaxonomyRoot>
</sov:SkillsTaxonomyOutput>
    */
                }

                //_sovrenApi.SubmitResumeAsync()


                return coursePage;
            }
        }

        public CourseDto ProcessCoursePage(CoursePage coursePage)
        {
            throw new NotImplementedException();
        }

        private class ITProTVCourse
        {
            public string Title { get; set; }
            public string Subtitle { get; set; }
            public string Description { get; set; }
            public string Duration { get; set; }
        }
    }
}
