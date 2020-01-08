using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class SubscriberEducationHistoryDto : BaseDto
    {
        public int SubscriberEducationHistoryId { get; set; }
        public Guid SubscriberEducationHistoryGuid { get; set; }
        public string EducationalInstitution  { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DegreeDate { get; set; }
        public Guid EducationalDegreeTypeGuid { get; set; } 
        public string EducationalDegreeType { get; set; } 
        public string EducationalDegree  { get; set; }        
    }
}
