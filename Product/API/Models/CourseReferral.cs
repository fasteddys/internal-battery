using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class CourseReferral : BaseModel
    {
        public int CourseReferralId { get; set; }
        public Guid CourseReferralGuid { get; set; }
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }
        public int ReferrerId { get; set; }
        public virtual Subscriber Referrer { get; set; }
        public int? RefereeId { get; set; }
        public virtual Subscriber Referee { get; set; }
        public string RefereeEmail { get; set; }
        public bool IsCourseViewed { get; set; }
    }
}
