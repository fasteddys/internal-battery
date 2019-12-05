using System;
using System.Collections.Generic;
using System.Text;

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
        public decimal? WeightedSkillScore { get; set; }
        public decimal? DistanceInMeters { get; set; }
    }
}
