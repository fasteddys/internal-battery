using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class SubscriberEducationAssessmentsDto
    {
        public List<SubscriberEducationDto> EducationHistories { get; set; } = new List<SubscriberEducationDto>();
        public List<SubscriberTrainingDto> TrainingHistories { get; set; } = new List<SubscriberTrainingDto>();

    }
}
