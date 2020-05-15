using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class EducationalHistoryDto
    {
        public List<EducationDto> EducationHistories { get; set; }
        public int TotalRecords { get; set; }
    }
}
