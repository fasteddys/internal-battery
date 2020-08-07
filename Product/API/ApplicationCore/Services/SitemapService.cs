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
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

            XDocument sitemap = new XDocument(
                new XDeclaration("1.0", "UTF-8", string.Empty),
                new XElement(ns + "urlset")
                );

            // generate static urls for the sitemap and append them to the document
            var staticSitemapXElements = new List<XElement>();
            staticSitemapXElements.Add(
                new XElement(ns + "url",
                    new XElement(ns + "loc", baseSiteUri),
                    new XElement(ns + "changefreq", "monthly")));
            staticSitemapXElements.Add(
                new XElement(ns + "url",
                    new XElement(ns + "loc", new Uri(baseSiteUri, "about")),
                    new XElement(ns + "changefreq", "monthly")));
            staticSitemapXElements.Add(
                new XElement(ns + "url",
                    new XElement(ns + "loc", new Uri(baseSiteUri, "offers")),
                    new XElement(ns + "changefreq", "monthly")));
            staticSitemapXElements.Add(
                new XElement(ns + "url",
                    new XElement(ns + "loc", new Uri(baseSiteUri, "contact")),
                    new XElement(ns + "changefreq", "monthly")));
            staticSitemapXElements.Add(
                new XElement(ns + "url",
                    new XElement(ns + "loc", new Uri(baseSiteUri, "privacy")),
                    new XElement(ns + "changefreq", "monthly")));
            staticSitemapXElements.Add(
                new XElement(ns + "url",
                    new XElement(ns + "loc", new Uri(baseSiteUri, "faq")),
                    new XElement(ns + "changefreq", "monthly")));
            staticSitemapXElements.Add(
                new XElement(ns + "url",
                    new XElement(ns + "loc", new Uri(baseSiteUri, "termsofservice")),
                    new XElement(ns + "changefreq", "monthly")));
            sitemap.Root.Add(staticSitemapXElements);

            // generate course-related urls for the sitemap and append them to the document
            var topics = await _repositoryWrapper.Topic.GetByConditionAsync(t => t.IsDeleted == 0 && !string.IsNullOrWhiteSpace(t.Slug));
            var courseSitemapXElements = new List<XElement>();
            courseSitemapXElements.Add(
                new XElement(ns + "url",
                    new XElement(ns + "loc", new Uri(baseSiteUri, "courses")),
                    new XElement(ns + "changefreq", "monthly")));
            courseSitemapXElements.AddRange(
                topics.Select(u =>
                    new XElement(ns + "url",
                        new XElement(ns + "loc", new Uri(baseSiteUri, "/courses/topics/" + u.Slug).ToString()),
                        new XElement(ns + "changefreq", "monthly")
                        )
                    )
                    .ToList()
                );
            sitemap.Root.Add(courseSitemapXElements);

            // generate job-related urls for the sitemap and append them to the document
            var jobSitemapUrls = await _repositoryWrapper.JobSite.GetAll()
                .Where(js => js.IsDeleted == 0)
                .Select(js => new JobSitemapDto { Url = new Uri(baseSiteUri, $"job/{js.JobSiteGuid}") })
                .ToListAsync();

            var jobSitemapXElements = jobSitemapUrls
                .Select(u =>
                    new XElement(ns + "url",
                        new XElement(ns + "loc", u.Url),
                        new XElement(ns + "changefreq", "daily")
                    )
                ).ToList();
            sitemap.Root.Add(jobSitemapXElements);
            
            // generate ButterCMS urls for the sitemap and append them to the document
            var butterSitemapXElements = new List<XElement>();
            var butterXmlDocument = await _butterCMSService.GetButterSitemapAsync();
            foreach (XmlNode location in butterXmlDocument.GetElementsByTagName("loc"))
            {
                butterSitemapXElements.Add(
                    new XElement(ns + "url",
                        new XElement(ns + "loc", location.InnerText),
                        new XElement(ns + "changefreq", "daily")
                    )
                );
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
    }
}
