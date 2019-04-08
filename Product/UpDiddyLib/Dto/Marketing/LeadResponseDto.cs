using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto.Marketing
{
    public class LeadResponseDto
    {
        public Guid? LeadIdentifier { get; set; }
        public bool IsBillable { get; set; }
        public List<LeadStatusDto> LeadStatuses { get; set; }
    }
}
