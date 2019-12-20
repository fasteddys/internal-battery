using System;
using System.Collections.Generic;
using System.Text;
using UpDiddyLib.Dto;

namespace UpDiddyLib.Domain.Models
{
    public class CourseReferralDto
    {
        public Guid CourseReferralGuid { get; set; }
        public Guid CourseGuid { get; set; }
        public Guid ReferrerGuid { get; set; }
        public string ReferralName { get; set; }
        public Guid RefereeGuid { get; set; }
        public string ReferralEmail { get; set; }  
        public bool IsCourseViewed { get; set; }
        public string ReferUrl { get; set; }    
        public string ReferralDescription { get; set; } 

    }
}
