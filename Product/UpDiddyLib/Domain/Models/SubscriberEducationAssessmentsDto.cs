using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class SubscriberEducationAssessmentsDto
    {
        public List<EducationalDegreeTypeDto> EducationalDegreeTypes { get; set; } = new List<EducationalDegreeTypeDto>();
        public List<TrainingTypeDto> TrainingTypes { get; set; } = new List<TrainingTypeDto>();

    }
}
