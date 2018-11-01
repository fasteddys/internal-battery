using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class WozCourseScheduleDto
    {
        public string CourseCode { get; set; }
        public IList<long> StartDatesUTC { get; set; }
        // todo: refactor this after go-live
        public List<Tuple<int, string, Decimal>> VariantToPrice { get; set; }
    }
}
