using System;
namespace UpDiddyLib.Domain.Models
{
    public class JobAlertDto
    {
        public Guid? JobAlertGuid {get;set;}
        public string Keywords { get; set; }
        public string Location {get;set;}
        public string Frequency { get; set; }
        public string Description { get; set; }
        public DayOfWeek? ExecutionDayOfWeek {get;set;}
    }
}