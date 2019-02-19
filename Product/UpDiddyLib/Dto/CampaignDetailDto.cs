using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class CampaignDetailDto
    {
        public string CourseName { get; set; }
        public string Rebatetype { get; set; }
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
