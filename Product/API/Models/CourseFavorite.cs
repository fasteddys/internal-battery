using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class CourseFavorite : BaseModel
    {
        public Guid CourseFavoriteGuid { get; set; }
        public int CourseFavoriteId { get; set; }
        public Course Course { get; set; }
        public int CourseId { get; set; }
        public int SubscriberId { get; set; }
        public Subscriber Subscriber { get; set; }
    }
}
