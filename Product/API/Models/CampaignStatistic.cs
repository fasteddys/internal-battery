using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    [NotMapped]
    public class CampaignStatistic
    {
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int EmailsSent { get; set; }
        public int OpenEmail { get; set; }
        public int VisitLandingPage { get; set; }
        public int CreateAcount { get; set; }
        public int CourseEnrollment { get; set; }
        public int CourseCompletion { get; set; }
        public Guid CampaignGuid { get; set; }
    }
}