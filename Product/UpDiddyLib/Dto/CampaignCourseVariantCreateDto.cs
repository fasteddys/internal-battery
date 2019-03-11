using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class  CampaignCourseVariantCreateDto
    {
        public int CampaignId { get; set; } 
        public int CourseVariantId { get; set; }        
        public int MaxRebateEligibilityInDays { get; set; }
        public bool IsEligibleForRebate { get; set; }
        public int RebateTypeId { get; set; } 
        public int? RefundId { get; set; } 
    }
}
