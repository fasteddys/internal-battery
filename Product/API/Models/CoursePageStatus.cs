using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class CoursePageStatus : BaseModel
    {
        public int CoursePageStatusId { get; set; }
        public Guid CoursePageStatusGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
