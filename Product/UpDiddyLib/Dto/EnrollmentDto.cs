using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{

    public enum EnrollmentStatus
    { 
         EnrollStudentRequested = 1, EnrollStudentInProgress = 11, EnrollStudentComplete = 21, EnrollStudentError = 31, EnrollStudentFatalError = 41,
         CreateSectionRequested = 2, CreateSectionInProgress = 12, CreateSectionComplete = 22, CreateSectionError = 32, CreateSectionFatalError = 42,
         RegisterStudentRequested = 3, RegisterStudentInProgress = 13, RegisterStudentComplete = 23, RegisterStudentError = 33, RegisterStudentFatalError = 43,
         FutureRegisterStudentRequested = 4, FutureRegisterStudentInProgress = 14, FutureRegisterStudentComplete = 24, FutureRegisterStudentFatalError = 44
    }
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
