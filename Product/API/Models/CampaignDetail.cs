using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    [NotMapped]
    public class CampaignDetail
    {
        public string CourseName { get; set; }
        public string Rebatetype{ get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int OpenEmail { get; set; }
        public int VisitLandingPage { get; set; }
        public int CreateAcount { get; set; }
        public int CourseEnrollment { get; set; }
        public int CourseCompletion { get; set; }
    }
}
