using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ISitemapService
    {
        Task<XDocument> GenerateSiteMap(Uri baseSiteUri);
        Task SaveSitemapToBlobStorage(XDocument sitemap);
    }
}
