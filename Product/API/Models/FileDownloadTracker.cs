using System;

namespace UpDiddyApi.Models
{
    public class FileDownloadTracker : BaseModel
    {
        public int FileDownloadTrackerId { get; set; }
        public Guid? FileDownloadTrackerGuid { get; set; }
        public int FileDownloadAttemptCount { get; set; }
        public Group Group { get; set; }
        public int? GroupId {get;set;}
        public int? MaxFileDownloadAttemptsPermitted { get; set; }
        public string SourceFileCDNUrl { get; set; }
        public DateTime? MostrecentfiledownloadAttemptinUtc { get; set; }
        public Subscriber Subscriber {get;set;}
        public int SubscriberId {get;set;}
    }
}
