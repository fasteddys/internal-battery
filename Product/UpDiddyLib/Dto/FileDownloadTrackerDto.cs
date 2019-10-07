using System;

namespace UpDiddyLib.Dto
{
    public class FileDownloadTrackerDto : BaseDto
    {
        public Guid? FileDownloadTrackerGuid { get; set; }
        public int FileDownloadAttemptCount { get; set; }
        public int MaxFileDownloadAttemptsPermitted { get; set; }
        public string SourceFileCDNUrl { get; set; }
        public DateTime? MostrecentfiledownloadAttemptinUtc { get; set; }
        public Guid? SubscriberGuid {get;set;}
    }
}
