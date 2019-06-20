using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{

    public enum ResumeParseSection { ContactInfo = 0, Skills, EducationHistory, WorkHistory }
    public class ResumeParseResultDto : BaseDto
    {
 
        public Guid ResumeParseResultGuid { get; set; }
 
        public virtual ResumeParseDto ResumeParse { get; set; }
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
