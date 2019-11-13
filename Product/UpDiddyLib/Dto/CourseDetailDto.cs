using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class CourseDetailDto
    {
        public string Title { get; set; }
        public string Duration { get; set; }
        public string Description { get; set; }
        public int CourseLevel { get; set; }
        public int NumLessons { get; set; }
        public string VendorName { get; set; }
        public int NumEnrollments { get; set; }
        public int NewFlag { get; set; }
        public Guid VendorGuid { get; set; }
        public string VendorLogoUrl { get; set; }

    }
    
}
