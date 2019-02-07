using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class CampaignCourseVariant : BaseModel
    {
        public int CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; }
        public int CourseVariantId { get; set; }
        public virtual CourseVariant CourseVariant { get; set; }
        public Guid? CampaignCourseVariantGuid { get; set; }
        public int? MaxRebateEligibilityInDays { get; set; }
        public bool IsEligibleForRebate { get; set; }
        public int RebateTypeId { get; set; }
        public virtual RebateType RebateType { get; set; }
        public int? RefundId { get; set; }
        public virtual Refund Refund { get; set; } 
    }
}
