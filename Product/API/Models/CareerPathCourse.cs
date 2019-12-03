using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class CareerPathCourse : BaseModel
    {
        public int CareerPathCourseId { get; set; }
        public Guid CareerPathCourseGuid { get; set; }
        public CareerPath CareerPath { get; set; }
        public int CareerPathId { get; set; }
        public Course Course { get; set; }
        public int CourseId { get; set; }
        public int Order { get; set; }
    }
}
