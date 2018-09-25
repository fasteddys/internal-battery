using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class VendorStudentLogin : BaseModel
    {
        public int VendorStudentLoginId { get; set; }
        public int VendorId { get; set; }
        public int SubscriberId { get; set; }
        public string VendorLogin { get; set; }
    }
}
