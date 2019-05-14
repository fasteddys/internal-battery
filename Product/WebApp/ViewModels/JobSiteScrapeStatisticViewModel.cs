using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddy.ViewModels
{
    public class JobSiteScrapeStatisticViewModel
    {
        public IEnumerable<JobSiteScrapeStatisticDto> Statistics { get; set; }
        public int NumRecords { get; set; } 
    }
}
