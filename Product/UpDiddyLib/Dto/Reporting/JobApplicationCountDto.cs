using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto.Reporting
{
    public class JobApplicationCountDto
    {
        public Guid CompanyGuid { get; set; }
        public string CompanyName { get; set; }
        public int ApplicationCount { get; set; }
    }
}
