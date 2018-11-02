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
        public int StatusCode { get; set; }
        public int PercentComplete { get; set; }
        public int EnrollmentStatusId { get; set; }

        private int _DisplayState;

        public int DisplayState
        {
            get { return _DisplayState; }
            set
            {
                if (value < 23)
                {
                    _DisplayState = -1;
                }
                else if (value < 25)
                {
                    _DisplayState = 0;
                }
                else
                {
                    _DisplayState = 1;
                }
            }
        }
    }
}
