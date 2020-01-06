using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpDiddyLib.Domain.Models
{
    public class RelatedJobDto
    {
        public Guid JobPostingGuid { get; set; }
        public DateTime PostingDateUTC { get; set; }
        public string CompanyName { get; set; }
        public string LogoUrl { get; set; }
        public string Title { get; set; }
        public string Industry { get; set; }
        public string JobCategory { get; set; }
        public string Country { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        [Column(TypeName = "decimal(10,5)")]
        public Decimal? WeightedSkillScore { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public Decimal? DistanceInMeters { get; set; }
        public int? DistanceIndex { get; set; }
    }
}
