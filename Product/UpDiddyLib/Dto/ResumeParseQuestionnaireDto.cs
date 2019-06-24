using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class ResumeParseQuestionnaireDto
    {
        public Guid ResumeParseGuid { get; set; }
        public IList<ResumeParseResultDto> EducationHistoryQuestions;
        public IList<ResumeParseResultDto> WorkHistoryQuestions;
        public IList<ResumeParseResultDto> ContactQuestions;
        public IList<ResumeParseResultDto> Skills;

    }
}
