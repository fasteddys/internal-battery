using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class BadgeCourse : BaseModel
    {
        public int BadgeCourseId { get; set; }
        public Guid? BadgeCourseGuid { get; set; }
        [Required]
        public string CourseId { get; set; }
        public string Notes { get; set; }
        public int? SortOrder { get; set; }
    }
}
