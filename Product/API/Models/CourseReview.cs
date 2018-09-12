using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class CourseReview : BaseModel
    {
        public int CourseReviewId { get; set; }
        public Guid? CourseReviewGuid { get; set; }
        public int CourseId { get; set; }
        public int SubscriberId { get; set; }
        public int Rating { get; set; }
        public string Review { get; set; }
        public int? ApprovedToPublish { get; set; }
        public int? ApprovedById { get; set; }
        public int VerifiedAttended { get; set; }
    }
}
