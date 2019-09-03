using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class CoursePage : BaseModel
    {
        public int CoursePageId { get; set; }
        public Guid CoursePageGuid { get; set; }
        [Required]
        public Uri Uri { get; set; }
        public int CoursePageStatusId { get; set; }
        public virtual CoursePageStatus CoursePageStatus { get; set; }
        public int? CourseId { get; set; }
        public virtual Course Course { get; set; }
        public int CourseSiteId { get; set; }
        public virtual CourseSite CourseSite { get; set; }
        [Required]
        public string UniqueIdentifier { get; set; }
        [Required]
        public string RawData { get; set; }
    }
}
