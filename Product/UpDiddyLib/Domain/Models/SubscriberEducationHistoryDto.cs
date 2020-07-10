using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class SubscriberEducationHistoryDto
    {
        public List<SubscriberEducationDto> EducationHistories { get; set; } = new List<SubscriberEducationDto>();
        public int TotalRecords { get; set; }
    }
}
