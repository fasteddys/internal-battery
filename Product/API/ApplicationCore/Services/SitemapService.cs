using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class SitemapService : ISitemapService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ICloudStorage _cloudStorage;

        public SitemapService(
            IRepositoryWrapper repositoryWrapper,
            ICloudStorage cloudStorage)
        {
            _repositoryWrapper = repositoryWrapper;
            _cloudStorage = cloudStorage;
        }

        public async Task<XDocument> GenerateSiteMap(Uri baseSiteUri)
        {
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

            XDocument sitemap = new XDocument(
                new XDeclaration("1.0", "UTF-8", string.Empty),
                new XElement(ns + "urlset")
                );

            // todo: static sitemap urls
            // todo: butter cms urls
            // todo: course urls

            var jobSitemapUrls = await _repositoryWrapper.StoredProcedureRepository.GetJobSitemapUrls(baseSiteUri);

            var jobSitemapXElements = jobSitemapUrls
                .Select(u => new XElement(ns + "url", new XElement(ns + "loc", u.Url), new XElement(ns + "changefreq", "daily")))
                .ToList();

            sitemap.Root.Add(jobSitemapXElements);

            return sitemap;
        }

        public async Task SaveSitemapToBlobStorage(XDocument sitemap)
        {
            using(var stream = new MemoryStream())
            {
                sitemap.Save(stream);
                var result = await _cloudStorage.UploadFileAsync("sitemap/", "sitemap.xml", stream);
            }
            

            throw new NotImplementedException();
        }
    }
}
