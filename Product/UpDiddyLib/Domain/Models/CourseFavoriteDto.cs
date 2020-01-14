using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models
{
    public class CourseFavoriteListDto
    {
        public List<CourseFavoriteDto> CourseFavorites { get; set; } = new List<CourseFavoriteDto>();
        public int TotalRecords { get; set; }
    }

    public class CourseFavoriteDto  
    {
        public string VendorLogoUrl { get; set; }
        public Guid CourseGuid {get;set;}
        public string Title { get; set; }
        public string Duration { get; set; }
        public string Description { get; set; }
        public Guid? CourseLevelGuid { get; set; }
        public string Level { get; set; }
        public string VendorName { get; set; }
        public int NumEnrollments { get; set; }
        public Guid VendorGuid { get; set; }
        public string TabletImage { get; set; }
        public string DesktopImage { get; set; }
        public string MobileImage { get; set; }
        public string ThumbnailImage { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}
