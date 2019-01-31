using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class CampaignCourseVariantDto
    { 
        public virtual CampaignDto Campaign { get; set; } 
        public virtual CourseVariantDto CourseVariant { get; set; }
        public int? MaxRebateEligibilityInDays { get; set; }
        public bool IsEligibleForRebate { get; set; }
        public virtual RebateTypeDto RebateType { get; set; }
        public virtual RefundDto Refund { get; set; }
    }
}
