using System;

namespace UpDiddyLib.Domain.Models
{
    public class CourseDetailDto
    {
        public string Title { get; set; }
        public string Duration { get; set; }
        public string Description { get; set; }
        public int NumEnrollments { get; set; }
        public string VendorName { get; set; }
        public Guid CourseGuid { get; set; }
        public string VendorLogoUrl { get; set; }
        public Guid VendorGuid { get; set; }
        public Guid? CourseLevelGuid { get; set; }
        public string Code { get; set; }
        public string Level { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }
        public int IsDeleted { get; set; }
        public string TabletImage { get; set; }
        public string DesktopImage { get; set; }
        public string MobileImage { get; set; }
        public string ThumbnailImage { get; set; }        
        public string Topic { get; set; }
        public string CourseSkills { get; set; }
    }
}
