using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class CompensationType : BaseModel
    {
        public int CompensationTypeId { get; set; }
        public Guid CompensationTypeGuid { get; set; }
        public string  CompensationTypeName { get; set; }
    }
}
