using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class EducationalHistoryDto
    {
        public List<EducationalDegreeDto> WorkHistories { get; set; } = new List<EducationalDegreeDto>();
        public int TotalRecords { get; set; }
    }
}
