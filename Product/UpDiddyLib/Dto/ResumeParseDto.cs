using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public enum ParseResumeStatus { Merged = 0, UnMerged, Declined, Duplicate, Error }
    public class ResumeParseDto
    {     
        public Guid ResumeParseGuid { get; set; }  
        public virtual SubscriberDto Subscriber { get; set; }    
        public virtual SubscriberFileDto SubscriberFile { get; set; }
        public int ParseStatus { get; set; }
    }
}
