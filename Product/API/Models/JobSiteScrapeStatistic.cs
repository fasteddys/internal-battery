using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class JobSiteScrapeStatistic : BaseModel
    {
        public int JobSiteScrapeStatisticId { set; get; }
        public Guid JobSiteScrapeStatisticGuid { set; get; }
        public int JobSiteId { get; set; }
        public virtual JobSite JobSite {get; set;}
        public DateTime ScrapeDate { get; set; }
        public int NumJobsProcessed { get; set; } 
        public int NumJobsAdded { get; set; }
        public int NumJobsDropped { get; set; }
        public int NumJobsUpdated { get; set; }
        public int NumJobsErrored { get; set; }

    }
}
