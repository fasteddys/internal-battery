using System;

namespace UpDiddyLib.Dto
{
    public class FileDto : BaseDto
    {
        public Guid? FileDownloadTrackerGuid { get; set; }
        public string FileName {get;set;}
        public byte[] Payload { get; set; }
        public string MimeType { get; set; }
        bool ExceededMaxDownloadAttempts {get;set;}
        public string ErrorMessage {get;set;}
    }
}
