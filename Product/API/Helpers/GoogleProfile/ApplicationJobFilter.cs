using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class ApplicationJobFilter
    {
        public string JobRequisitionId { get; set; }
        public string JobTitle { get; set; }
        public bool Negated { get; set; }

    }
}
