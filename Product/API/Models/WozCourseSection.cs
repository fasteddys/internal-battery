using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class WozCourseSection : BaseModel
    {
        public int WozCourseSectionId { get; set; }
        public string CourseCode { get; set; }
        public int Section { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
