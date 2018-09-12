using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class TagCourse : BaseModel
    {
        public int TagCourseId { get; set; }
        public Guid? TagCourseGuid { get; set; }
        public int TagId { get; set; }
        public int CourseId { get; set; }

    }
}
