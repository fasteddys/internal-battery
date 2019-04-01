using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class LeadStatus : BaseModel
    {
        public int LeadStatusId { get; set; }
        public Guid? LeadStatusGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Severity Severity { get; set; }
    }

    public enum Severity
    {
        Unknown = 0,
        Accepted = 1,
        Warning = 2,
        Rejected = 3
    }
}
