using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class CourseSite : BaseModel
    {
        public int CourseSiteId { get; set; }
        public Guid CourseSiteGuid { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public Uri Uri { get; set; }
        public DateTime? LastCrawl { get; set; }
        public DateTime? LastSync { get; set; }
        public List<CoursePage> CoursePages { get; set; } = new List<CoursePage>();
        public bool IsCrawling { get; set; }
        public bool IsSyncing { get; set; }
    }
}
