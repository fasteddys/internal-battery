using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpDiddyLib.Domain.Models
{
    public class RecruiterStat
    {
        public int RecruiterStatId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [DefaultValue(0)]
        public int OpCoSubmittals { get; set; }
        [DefaultValue(0)]
        public int CCSubmittals { get; set; }
        [DefaultValue(0)]
        public int OpCoInterviews { get; set; }
        [DefaultValue(0)]
        public int CCInterviews { get; set; }
        [DefaultValue(0)]
        public int OpCoStarts { get; set; }
        [DefaultValue(0)]
        public int CCStarts { get; set; }
        [DefaultValue(0)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OpCoSpread { get; set; }
        [DefaultValue(0)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CCSpread { get; set; }
    }
}