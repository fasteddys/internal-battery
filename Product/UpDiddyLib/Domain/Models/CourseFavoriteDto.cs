using System;
namespace UpDiddyLib.Domain.Models
{
    public class CourseFavoriteDto  
    {
        public string VendorLogoUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public Guid CourseGuid {get;set;}
        public string Title { get; set; }
        public string Duration { get; set; }
        public string Description { get; set; }
        public Guid CourseLevelGuid { get; set; }
        public string VendorName { get; set; }
        public int NumEnrollments { get; set; }
        public Guid VendorGuid { get; set; }
    }
}
