using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Shared;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class SitemapService : ISitemapService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ICloudStorage _cloudStorage;
        private readonly IButterCMSService _butterCMSService;

        private static readonly XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

        public SitemapService(
            IRepositoryWrapper repositoryWrapper,
            ICloudStorage cloudStorage,
            IButterCMSService butterCMSService)
        {
            _repositoryWrapper = repositoryWrapper;
            _cloudStorage = cloudStorage;
            _butterCMSService = butterCMSService;
        }

        public async Task<XDocument> GenerateSiteMap(Uri baseSiteUri)
        {
            XDocument sitemap = new XDocument(
                new XDeclaration("1.0", "UTF-8", string.Empty),
                new XElement(ns + "urlset")
                );

            // generate static urls for the sitemap and append them to the document

            var url = GetUrlFactory(baseSiteUri); // putting baseSiteUrl into a closure in order to simplify this code a little bit.

            var staticSitemapXElements = new List<XElement>
            {
                url(string.Empty, "monthly"),
                url("about", "monthly"),
                url("offers", "monthly"),
                url("contact", "monthly"),
                url("privacy", "monthly"),
                url("faq", "monthly"),
                url("termsofservice", "monthly")
            };
            sitemap.Root.Add(staticSitemapXElements);

            // generate course-related urls for the sitemap and append them to the document
            var topics = await _repositoryWrapper.Topic.GetByConditionAsync(t => t.IsDeleted == 0 && !string.IsNullOrWhiteSpace(t.Slug));
            var courseSitemapXElements = new List<XElement>
            {
                url("courses", "monthly")
            };

            courseSitemapXElements.AddRange(topics
                    .Select(u => url($"/courses/topics/{u.Slug}", "monthly"))
                    .ToList());

            sitemap.Root.Add(courseSitemapXElements);

            // generate job-related urls for the sitemap and append them to the document
            var jobSitemapXElements = await _repositoryWrapper.JobPosting.GetAll()
                .Where(jp => jp.IsDeleted == 0)
                .Select(jp => url($"job/{jp.JobPostingGuid}", "daily"))
                .ToListAsync();

            sitemap.Root.Add(jobSitemapXElements);
            
            // generate ButterCMS urls for the sitemap and append them to the document
            var butterSitemapXElements = new List<XElement>();
            var butterXmlDocument = await _butterCMSService.GetButterSitemapAsync();
            foreach (XmlNode location in butterXmlDocument.GetElementsByTagName("loc"))
            {
                butterSitemapXElements.Add(GetUrl(location.InnerText, "daily"));
            }
            sitemap.Root.Add(butterSitemapXElements);

            return sitemap;
        }

        public async Task SaveSitemapToBlobStorage(XDocument sitemap)
        {
            using (var stream = new MemoryStream())
            {
                Encoding encoding = Encoding.UTF8;
                UTF8StringWriter sw = new UTF8StringWriter();
                XmlTextWriter xw = new XmlTextWriter(sw);
                sitemap.WriteTo(xw);
                byte[] docAsBytes = encoding.GetBytes(sw.ToString());
                var result = await _cloudStorage.UploadBlobAsync("sitemap.xml", docAsBytes);
            }
        }

        private static Func<string, string, XElement> GetUrlFactory(Uri baseSiteUri)
            => (location, changeFreq) => GetUrl((string.IsNullOrEmpty(location) ? baseSiteUri : new Uri(baseSiteUri, location)).ToString(), changeFreq);

        private static XElement GetUrl(string location, string changeFreq)
            => new XElement(ns + "url",
                new XElement(ns + "loc", location),
                new XElement(ns + "changefreq", changeFreq));
    }
}
