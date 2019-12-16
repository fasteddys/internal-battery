using System;
namespace UpDiddyLib.Domain.Models
{
    public class ExistingUserSignupDto
    {
        public string ReferrerUrl { get; set; }
        public string CampaignSlug { get; set; }
        public bool? IsWaitlist { get; set; }
        public bool? IsGatedDownload { get; set; }
        public string GatedDownloadFileUrl { get; set; }
        public int? GatedDownloadMaxAllowedDownloadAttempt { get; set; }
    }
}