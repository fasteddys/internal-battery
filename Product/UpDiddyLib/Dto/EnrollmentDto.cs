using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class EnrollmentDto : BaseDto
    {
        public int EnrollmentId { get; set; }
        public Guid? EnrollmentGuid { get; set; }
        public int CourseId { get; set; }
        public CourseDto Course { get; set; }
        public int SubscriberId { get; set; }
        public SubscriberDto Subscriber { get; set; }
        public DateTime DateEnrolled { get; set; }
        public Decimal PricePaid { get; set; }
        public int PercentComplete { get; set; }
        public int? IsRetake { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime? DroppedDate { get; set; }
        public int EnrollmentStatusId { get; set; }
        public int? TermsOfServiceFlag { get; set; }
    }
}
