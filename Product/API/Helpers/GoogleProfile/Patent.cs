using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class Patent
    {
        public string displayName { get; set; }
        public List<string> inventors { get; set; }
        public string patentStatus { get; set; }

        public Date patentStatusDate { get; set; }

        public Date patenetFilingDate { get; set; }

        public string patentOffice { get; set; }

        public string patentNumber { get; set; }

        public string patentDescription { get; set; }

        public List<Skill> skillsUsed { get; set; }
    }
}
