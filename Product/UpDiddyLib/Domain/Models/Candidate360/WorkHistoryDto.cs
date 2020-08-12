using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models.Candidate360
{
    public class WorkHistoryDto
    {
        public Guid? WorkHistoryGuid { get; set; }
        public string CompanyName { get; set; }
        public string JobTitle { get; set; }
        public string JobDescription { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsCurrent { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }

    public class WorkHistoryListDto
    {
        public List<WorkHistoryDto> WorkHistories { get; set; } = new List<WorkHistoryDto>();
        public int TotalRecords { get; set; }
    }

    public class WorkHistoryUpdateDto
    {
        public List<WorkHistoryDto> WorkHistories { get; set; } = new List<WorkHistoryDto>();
    }
}