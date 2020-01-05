using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class CourseVariantCheckoutDto
    {
        public Guid CourseVariantGuid { get; set; }
        public string CourseVariantType { get; set; }

        public Decimal Price { get; set; }

        public Dictionary<string,string> StartDates { get; set; }

        public bool IsElgibleCampaignOffer { get; set; }

        public string RebateOffer { get; set; }

        public string RebateTerms { get; set; } 
    }
}
