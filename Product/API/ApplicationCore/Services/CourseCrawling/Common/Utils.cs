using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using UpDiddyApi.ApplicationCore.Interfaces;

namespace UpDiddyApi.ApplicationCore.Services.CourseCrawling.Common
{
    public static class Utils
    {
        public static List<string> ParseSkillsFromSovren(ISovrenAPI sovrenApi, string title, string subtitle, string description, string overview)
        {
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms))
                {
                    sw.WriteLine("Title");
                    sw.WriteLine(title);
                    sw.WriteLine("Subtitle");
                    sw.WriteLine(subtitle);
                    sw.WriteLine("Description");
                    sw.WriteLine(description);
                    sw.WriteLine("Overview");
                    sw.WriteLine(overview);
                    sw.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    bytes = ms.ToArray();
                }
            }
            string base64 = Convert.ToBase64String(bytes);
            var sovrenResult = sovrenApi.SubmitResumeAsync(-1, base64).Result;

            var doc = XDocument.Parse(sovrenResult);
            XNamespace xmlns = "http://ns.hr-xml.org/2006-02-28";
            XNamespace sov = "http://sovren.com/hr-xml/2006-02-28";

            return doc.Root
                .Element(xmlns + "UserArea")
                .Descendants(sov + "Skill")
                .Attributes("name")
                .Select(a => a.Value)
                .ToList();
        }
    }
}
