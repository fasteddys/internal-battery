using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
   public class WozCourseProgress
    {
        public string LetterGrade { get; set; }
        public int PercentageGrade { get; set; }
        public int ActivitiesCompleted { get; set; }
        public int ActivitiesTotal { get; set; }
        public string CourseName { get; set; }
        public string CourseUrl { get; set; }
    }
}
