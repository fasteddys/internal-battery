using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class WozCourseEnrollmentDto : BaseDto
    {
        public int WozCourseEnrollmentId { get; set; }
        public int WozEnrollmentId { get; set; }
        public int SectionId { get; set; }
        public int EnrollmentStatus { get; set; }
        public int ExeterId { get; set; }
        public long EnrollmentDateUTC { get; set; }
        public Guid EnrollmentGuid { get; set; }
    }
}
