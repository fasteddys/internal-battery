using Newtonsoft.Json;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models.Reports
{
    public class RecruiterStatListDto
    {
        public List<RecruiterStatDto> RecruiterStats { get; set; } = new List<RecruiterStatDto>();
        public int TotalOpCoSubmittals { get; set; }
        public int TotalCCSubmittals { get; set; }
        public int TotalOpCoInterviews { get; set; }
        public int TotalCCInterviews { get; set; }
        public int TotalOpCoStarts { get; set; }
        public int TotalCCStarts { get; set; }
        public decimal TotalOpCoSpread { get; set; }
        public decimal TotalCCSpread { get; set; }
    }

    public class RecruiterStatDto
    {
        public int RecruiterStatId { get; set; }
        public string DateRange { get; set; }
        public int OpCoSubmittals { get; set; }
        public int CCSubmittals { get; set; }
        public int OpCoInterviews { get; set; }
        public int CCInterviews { get; set; }
        public int OpCoStarts { get; set; }
        public int CCStarts { get; set; }
        public decimal OpCoSpread { get; set; }
        public decimal CCSpread { get; set; }
        [JsonIgnore]
        public int TotalOpCoSubmittals { get; set; }
        [JsonIgnore]
        public int TotalCCSubmittals { get; set; }
        [JsonIgnore]
        public int TotalOpCoInterviews { get; set; }
        [JsonIgnore]
        public int TotalCCInterviews { get; set; }
        [JsonIgnore]
        public int TotalOpCoStarts { get; set; }
        [JsonIgnore]
        public int TotalCCStarts { get; set; }
        [JsonIgnore]
        public decimal TotalOpCoSpread { get; set; }
        [JsonIgnore]
        public decimal TotalCCSpread { get; set; }
    }
}
