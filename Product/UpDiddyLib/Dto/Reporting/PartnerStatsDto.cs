using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto.Reporting
{
    public class PartnerStatsDto
    {
        public string PartnerName {get; set;}
        public Dictionary<string, int> Stats { get; set;}

    }
}
