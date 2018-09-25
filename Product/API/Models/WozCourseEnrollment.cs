using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class WozCourseEnrollment: BaseModel
    {
        public int WozCourseEnrollmentId { get; set; }
        public int WozEnrollmentId { get; set; }
        public int SectionId { get; set; }
        public int EnrollmentStatus { get; set; }
        public int ExeterId { get; set; }
        public long EnrollmentDateUTC { get; set; }
        public int EnrollmentId { get; set; }
    }
}
