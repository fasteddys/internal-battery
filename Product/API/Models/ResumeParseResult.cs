using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class ResumeParseResult : BaseModel
    {
        public int ResumeParseResultId { get; set; }
        public Guid ResumeParseResultGuid { get; set; } 
        public int ResumeParseId { get; set; }
        public virtual ResumeParse ResumeParse { get; set; }
        public int ParseStatus { get; set; }
        public string TargetTypeName { get; set; }
        public string TargetProperty { get; set; }
        public string ParsedValue { get; set; }

        public string ExistingValue { get; set; }

        public Guid ExistingObjectGuid { get; set; }

        public string Prompt { get; set; } 

        public int ProfileSectionId { get; set; }


        public string ProcessingMessage { get; set; }

    }
}
