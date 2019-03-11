using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class CampaignCreateDto
    {
        public string Name { get; set; }
        public IList<CampaignCourseVariantCreateDto> CourseVariants { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public string Terms { get; set; }
        public string PhaseName { get; set; }
        public string PhaseDescription {get; set; }

    }
}
