using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto.Reporting
{
    public class PCAReportDto
    {
        public List<PartnerStatsDto> Report {get; set;}
        public List<ActionKeyDto> ActionKey {get; set;}
    }
}
