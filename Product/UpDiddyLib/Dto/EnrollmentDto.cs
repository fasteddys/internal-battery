using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public enum SimplifiedEnrollmentStatus
    {
        Unknown = 0,
        Pending = 1,
        Success = 2, 
        Error = 3
    }
    public enum EnrollmentStatus
    { 
         PaymentInitiated = 0, PaymentInProgress = 10, PaymentComplete = 20, PaymentError = 30, PaymentFatalError = 40,
         EnrollStudentRequested = 1, EnrollStudentInProgress = 11, EnrollStudentComplete = 21, EnrollStudentError = 31, EnrollStudentFatalError = 41,
         CreateSectionRequested = 2, CreateSectionInProgress = 12, CreateSectionComplete = 22, CreateSectionError = 32, CreateSectionFatalError = 42,
         RegisterStudentRequested = 3, RegisterStudentInProgress = 13, RegisterStudentComplete = 23, RegisterStudentError = 33, RegisterStudentFatalError = 43,
         FutureRegisterStudentRequested = 4, FutureRegisterStudentInProgress = 14, FutureRegisterStudentComplete = 24, FutureRegisterStudentError = 34, FutureRegisterStudentFatalError = 44
    }
    public class EnrollmentDto : BaseDto
    {
        public int EnrollmentId { get; set; }
        public Guid? EnrollmentGuid { get; set; }
        public int CourseId { get; set; }
        public CourseDto Course { get; set; }
        public int SubscriberId { get; set; }
        [JsonIgnore]
        public SubscriberDto Subscriber { get; set; }
        public DateTime DateEnrolled { get; set; }
        public Decimal PricePaid { get; set; }
        public int PercentComplete { get; set; }
        public int? IsRetake { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime? DroppedDate { get; set; }
        public int EnrollmentStatusId { get; set; }
        public int? TermsOfServiceFlag { get; set; }
        public Int64? SectionStartTimestamp { get; set; }
        public Guid? PromoCodeRedemptionGuid { get; set; }
        public int? CourseVariantId { get; set; }
        public Guid CourseVariantGuid { get; set; }
        public Guid CourseGuid { get; set; }
        public string CourseUrl { get; set; }
        public virtual CampaignDto Campaign { get; set; }

        public string CampaignPromoInfo { get; set; }

        public SimplifiedEnrollmentStatus SimplifiedEnrollmentStatus
        {
            get
            {

                if (this.EnrollmentStatusId < 23)
                    return SimplifiedEnrollmentStatus.Pending;
                else if (this.EnrollmentStatusId < 25)
                    return SimplifiedEnrollmentStatus.Success;
                else
                    return SimplifiedEnrollmentStatus.Error;
            }
        }
    }
}
