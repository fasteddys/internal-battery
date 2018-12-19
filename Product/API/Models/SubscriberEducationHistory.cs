using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class SubscriberEducationHistory : BaseModel
    {
        public int SubscriberEducationHistoryId { get; set; }
        public Guid SubscriberEducationHistoryGuid  { get; set; }
        public int SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }
        public int EducationalInstitutionId { get; set; }
        public virtual EducationalInstitution EducationalInstitution { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DegreeDate { get; set; }
        public int EducationalDegreeTypeId { get; set; }
        public virtual EducationalDegreeType EducationalDegreeType { get; set; }
        public int EducationalDegreeId { get; set; }
        public virtual EducationalDegree EducationalDegree { get; set; }

    }
}
