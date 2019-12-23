using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class RelatedCourseDto
    {
        public Guid CourseGuid { get; set; }
        public string CourseName { get; set; }
        public string CourseDescription { get; set; }        
        public string VendorName { get; set; }
        public string VendorLogoUrl { get; set; }
        [Column(TypeName = "decimal(10,5)")]
        public Decimal? WeightedSkillScore { get; set; }
        public string TabletImage { get; set; }
        public string DesktopImage { get; set; }
        public string MobileImage { get; set; }
        public string ThumbnailImage { get; set; }
    }
}
