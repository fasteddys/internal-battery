using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
 
    public class PartnerDto : BaseDto
    {
        public Guid? PartnerGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
    }
}
