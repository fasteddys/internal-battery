using System;
using System.ComponentModel.DataAnnotations;

namespace UpDiddyLib.Domain.Models
{
    public class SubscriberDto
    {
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        public string ReferrerUrl { get; set; }

        public string JobReferralCode { get; set; }

        public Guid PartnerGuid { get; set; }

        public bool IsWaitlist {get;set;}
                
        public bool IsGatedDownload { get; set; }

        public string GatedDownloadFileUrl { get; set; }

        public int? GatedDownloadMaxAttemptsAllowed { get; set; }

        public string AssessmentId { get; set; }

    }
}
