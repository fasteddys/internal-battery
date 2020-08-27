using System;

namespace UpDiddyLib.Domain.Models.CrossChq
{
    public class CrossChqCandidateStatusDto
    {
        public Guid ProfileGuid { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime SubscriberCreateDate { get; set; }

        public DateTime? ResumeUploadedDate { get; set; }

        public TimeSpan? ElapsedTimeToUploadResume { get; set; }

        public string CrossChqReferenceCheckType { get; set; }

        public string CrossChqJobTitle { get; set; }

        public DateTime? CrossChqStatusDate { get; set; }

        public int? CrossChqPercentage { get; set; }

        public int? CrossChqStatus { get; set; }
    }

    public class CrossChqCandidateStatusListDto
        : BaseListDto<CrossChqCandidateStatusDto>
    { }
}
