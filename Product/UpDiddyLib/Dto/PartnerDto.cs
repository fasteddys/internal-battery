using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class PartnerDto : BaseDto
    {
        public int PartnerId { get; set; }
        public Guid? PartnerGuid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
