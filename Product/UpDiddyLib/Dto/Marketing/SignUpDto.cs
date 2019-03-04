using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto.Marketing
{
    public class SignUpDto : BaseDto
    {
        public string email { get; set; }
        public string password { get; set; }
        public Guid? campaignGuid { get; set; }
        public string campaignPhase { get; set; }
    }
}
 