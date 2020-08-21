using System;

namespace UpDiddyLib.Domain.Models.CrossChq
{
    public class CrossChqCandidateStatusDto
    {
        public Guid ProfileGuid { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string EmailAddress { get; set; }

        public CrossChqStatus CrossChqStatus { get; set; }

        public DateTime? ResumeUploadedDate { get; set; }
    }

    public enum CrossChqStatus
    {
    }

    public class CrossChqCandidateStatusListDto
        : BaseListDto<CrossChqCandidateStatusDto>
    { }
}
