using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class WozCourseScheduleDto
    {
        public string CourseCode { get; set; }
        public IList<long> StartDatesUTC { get; set; }
        public Dictionary<string, Decimal> VariantToPrice { get; set; }
    }
}
