using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class CourseDetailDto : CourseBaseDto
    {
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
