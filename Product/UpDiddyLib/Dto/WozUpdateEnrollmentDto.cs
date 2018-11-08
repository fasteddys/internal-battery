using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{


    public enum WozEnrollmentStatus { Enrolled = 100, Graduated = 200, Incomplete = 300, Withdrawn = 400, WithdrawnWithFailure = 450, Canceled = 500 }

    public class WozUpdateEnrollmentDto
    {
      
        public int enrollmentStatus { get; set; }
        public long enrollmentDateUTC { get; set; }
        public long removalDateUTC { get; set; }

    }
}
