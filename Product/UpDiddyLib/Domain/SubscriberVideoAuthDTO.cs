using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain
{
    public class SubscriberVideoAuthDTO
    {
        public string VideoSAS { get; set; }
        public string VideoThumbnailSAS { get; set; }
        public string VideoURI { get; set; }
        public string VideoThumbnailURI { get; set; }
    }
}
