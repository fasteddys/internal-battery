using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class CompensationTypeDto : BaseDto
    {
        public Guid CompensationTypeGuid { get; set; }
        public string CompensationTypeName { get; set; }
    }
}
