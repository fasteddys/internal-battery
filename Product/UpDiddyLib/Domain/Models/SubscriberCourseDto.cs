using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class SubscriberCourseDto
    {
        public int Grade { get; set; }
        public int PercentComplete { get; set; }
        public string Title { get; set; }

        public string Description { get; set; }

        public string CourseLevel { get; set; }
 
        public string VendorName { get; set; }

        public Guid VendorGuid { get; set; }

        public string VendorLogoUrl { get; set; }

        public int EnrollmentStatusId { get; set; }
    }
}
