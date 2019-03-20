using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class SecurityClearanceDto : BaseDto
    {
      
        public Guid SecurityClearanceGuid { get; set; }
        public string Name { get; set; }
    }
}
