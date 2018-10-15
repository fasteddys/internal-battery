using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class VendorStudentLoginDto : BaseDto
    {           
        public int VendorStudentLoginId { get; set; }
        public int VendorId { get; set; }
        public int SubscriberId { get; set; }
        public string VendorLogin { get; set; }
        public string RegistrationUrl { get; set; }
    }    
}
