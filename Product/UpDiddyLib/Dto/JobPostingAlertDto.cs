using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class JobPostingAlertDto
    {
        public string Description { get; set; }
        public int ExecutionHour { get; set; }
        public int ExecutionMinute { get; set; }
        public string Frequency { get; set; } // currently supports Daily and Weekly
        public JobQueryDto JobQuery { get; set; }
        public DayOfWeek? ExecutionDayOfWeek { get; set; } // only used for Weekly frequency
        public int TimeZoneOffset { get; set; }
        public string LocalDate { get; set; }
        public Guid? JobPostingAlertGuid { get; set; }
        public SubscriberDto Subscriber { get; set; }
    }
}
