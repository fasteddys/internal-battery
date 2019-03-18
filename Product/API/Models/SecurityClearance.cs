using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class SecurityClearance
    {
        public int SecurityClearanceId { get; set; }
        public Guid SecurityClearanceGuid { get; set; }
        public string Name { get; set; }
    }
}
