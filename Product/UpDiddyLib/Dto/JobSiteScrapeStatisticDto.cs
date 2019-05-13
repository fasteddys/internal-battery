using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class JobSiteScrapeStatisticDto
    {

        public Guid JobSiteScrapeStatisticGuid { set; get; } 
        public virtual JobSiteDto JobSite { get; set; }
        public DateTime ScrapeDate { get; set; }
        public int NumJobsProcessed { get; set; }
        public int NumJobsAdded { get; set; }
        public int NumJobsDropped { get; set; }
        public int NumJobsUpdated { get; set; }
        public int NumJobsErrored { get; set; }

    }
}
