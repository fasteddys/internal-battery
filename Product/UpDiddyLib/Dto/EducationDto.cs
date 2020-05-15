using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class EducationDto
	{
        public Guid? EducationHistoryGuid { get; set; }
		public Guid? EducationalInstitutionGuid { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public DateTime? DegreeDate { get; set; }
		public Guid? EducationalDegreeTypeGuid { get; set; }
		public Guid? EducationalDegreeGuid { get; set; }
    }
}
