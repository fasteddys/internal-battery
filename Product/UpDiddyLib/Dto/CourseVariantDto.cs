using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class CourseVariantDto : BaseDto
    {
        public int CourseVariantId { get; set; }
        public Guid CourseGuid { get; set; }
        public Decimal Price { get; set; }
        public string VariantType { get; set; }
    }
}
