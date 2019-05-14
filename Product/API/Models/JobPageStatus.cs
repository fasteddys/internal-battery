using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class JobPageStatus : BaseModel
    {
        public int JobPageStatusId { get; set; }
        public Guid JobPageStatusGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
