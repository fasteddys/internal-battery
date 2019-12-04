using System;
namespace UpDiddyLib.Domain.Models
{
    public class TopicCourseDto
    {
        public Guid CourseGuid {get;set;}
        public string Name { get; set; }
        public string Description { get; set; }
        public string VendorLogoUrl { get; set; }

    }
}