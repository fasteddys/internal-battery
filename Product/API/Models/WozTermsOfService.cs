using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class WozTermsOfService : BaseModel
    {
        public int WozTermsOfServiceId { get; set; }
        public int DocumentId { get; set; }
        public string TermsOfService { get; set; }
    }
}
