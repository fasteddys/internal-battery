using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class CourseSiteDto 
    {
        public Guid CourseSiteGuid { get; set; }
        public string Name { get; set; }
        public Uri Uri { get; set; }
        public DateTime? LastCrawl { get; set; }
        public DateTime? LastSync { get; set; }
        public int SyncCount { get; set; }
        public int UpdateCount { get; set; }
        public int CreateCount { get; set; }
        public int DeleteCount { get; set; }
        public int ErrorCount { get; set; }
    }
}
