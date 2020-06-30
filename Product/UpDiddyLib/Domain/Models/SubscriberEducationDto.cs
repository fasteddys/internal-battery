using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class SubscriberEducationDto
	{
		public Guid EducationHistoryGuid { get; set; }
		public Guid EducationalDegreeTypeGuid { get; set; }
		public string EducationalDegreeType { get; set; }
		public string Institution { get; set; }
		public short? RelevantYear { get; set; }
		public string EducationalDegree { get; set; }
    }
}
