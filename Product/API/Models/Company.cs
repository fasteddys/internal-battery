using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public partial class Company : BaseModel
    {
        public int CompanyId { get; set; }
        public Guid CompanyGuid { get; set; }
        public string CompanyName { get; set; }
    }
}
