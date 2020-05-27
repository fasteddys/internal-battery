using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
   
    public class EducationDto
	{
        public Guid? EducationHistoryGuid { get; set; }		
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public DateTime? DegreeDate { get; set; }
	    public string Institution { get; set; }
        public string EducationalDegree { get; set; }
    }


}
