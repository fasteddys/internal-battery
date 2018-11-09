using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class CourseVariantTypeDto : BaseDto
    {
        public int CourseVariantTypeId { get; set; }
        public Guid? CourseVariantTypeGuid { get; set; }
        public string Name { get; set; }
    }
}
