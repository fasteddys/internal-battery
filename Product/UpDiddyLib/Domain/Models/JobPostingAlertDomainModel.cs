namespace UpDiddyLib.Domain.Models
{
    public class JobPostingAlertDomainModel
    {
        public string Description { get; set; }
        public int ExecutionHour { get; set; }
        public int ExecutionMinute { get; set; }
        public string Frequency { get; set; } // currently supports Daily and Weekly
        public JobQuery JobQuery { get; set; }
        public string ExecutionDayOfWeek { get; set; } // only used for Weekly frequency
        public int TimeZoneOffset { get; set; }
        public string LocalDate { get; set; }
    }
}