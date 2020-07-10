using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class SubscriberTrainingDto
    {
        public Guid? SubscriberTrainingGuid { get; set; }

        public Guid TrainingTypeGuid { get; set; }

        public short? RelevantYear { get; set; }

        public string TrainingInstitution { get; set; }

        public string TrainingName { get; set; }
    }
}
