using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public enum ResumeParseStatus { Merged = 0, MergeNeeded, Declined, Duplicate, Error }
    public class ResumeParseDto
    {     
        public Guid ResumeParseGuid { get; set; }  
        public virtual SubscriberDto Subscriber { get; set; }    
        public virtual SubscriberFileDto SubscriberFile { get; set; }
        public int ParseStatus { get; set; }
        public int RequiresMerge { get; set; }
        public IList<ResumeParseResultDto> resumeParseResults { get; set; }

    }
}
