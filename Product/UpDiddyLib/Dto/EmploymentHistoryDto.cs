using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class EmploymentHistoryDto
    {
        public List<EmploymentDto> WorkHistories { get; set; } = new List<EmploymentDto>();
        public int TotalRecords { get; set; }
    }
}
