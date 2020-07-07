using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class SubscriberTrainingHistoryDto
    {
        public List<SubscriberTrainingDto> TrainingHistories { get; set; } = new List<SubscriberTrainingDto>();
        public int TotalRecords { get; set; }

    }
}
