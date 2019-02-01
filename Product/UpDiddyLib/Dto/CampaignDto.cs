using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class CampaignDto : BaseDto
    {
        public int CampaignId { get; set; }
        public Guid CampaignGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<CampaignCourseVariantDto> CampaignCourseVariant { get; set; } = new List<CampaignCourseVariantDto>();


    }
}
