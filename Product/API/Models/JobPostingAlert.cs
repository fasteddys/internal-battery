using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public enum Frequency { Daily = 0, Weekly = 1 }

    public class JobPostingAlert : BaseModel
    {
        public int JobPostingAlertId { get; set; }
        public Guid JobPostingAlertGuid { get; set; }
        [Required]
        [MaxLength(250)]
        public string Description { get; set; }
        public int SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }
        [Required]
        [Range(0, 23)]
        public int ExecutionHour { get; set; }
        [Range(0, 59)]
        public int ExecutionMinute { get; set; }
        public DayOfWeek? ExecutionDayOfWeek { get; set; }
        public Frequency Frequency { get; set; }
        // https://docs.microsoft.com/en-us/ef/core/modeling/backing-field
        private string _jobQueryDto;
        [NotMapped]
        public JObject JobQueryDto
        {
            get
            {
                return JsonConvert.DeserializeObject<JObject>(string.IsNullOrEmpty(_jobQueryDto) ? "{}" : _jobQueryDto);
            }
            set
            {
                _jobQueryDto = value.ToString();
            }
        }
    }
}
