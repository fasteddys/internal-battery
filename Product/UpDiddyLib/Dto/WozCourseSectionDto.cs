using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class WozCourseSectionDto : BaseDto
    {
        public int WozCourseSectionId { get; set; }
        public string CourseCode { get; set; }
        public int Section { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }


}
