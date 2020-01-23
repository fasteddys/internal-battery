using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models
{
    public class JobSiteScrapeStatisticsListDto
    {
        public List<JobSiteScrapeStatisticDto> JobSiteScrapeStatistics { get; set; } = new List<JobSiteScrapeStatisticDto>();
        public int TotalRecords { get; set; }
    }

    public class JobSiteScrapeStatisticDto
    {
        public string JobSite { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int NumJobsAdded { get; set; }
        public int NumJobsUpdated { get; set; }
        public int NumJobsDropped { get; set; }
        public int NumJobsErrored { get; set; }
        public int NumJobsProcessed { get; set; }
        public int MinutesElapsed { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}
