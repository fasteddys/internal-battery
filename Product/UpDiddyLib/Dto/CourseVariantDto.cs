using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class CourseVariantDto : BaseDto
    {
        public int CourseVariantId { get; set; }
        public Guid CourseVariantGuid { get; set; }
        public Decimal Price { get; set; }
        public CourseVariantTypeDto CourseVariantType { get; set; }
        // todo: does this belong somewhere else, should we use polymorphism to return different types of course variants based on vendor?
        public List<DateTime> StartDateUTCs { get; set; }
    }
}
