using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto.Reporting
{
    public class RecruiterActionSummaryDto
    {
        public string RecruiterEmail { get; set; }
        public string RecruiterFirstName { get; set; }
        public string RecruiterLastName { get; set; }
        public string Action { get; set; }
        public int ActionCount { get; set; }
    }
}
