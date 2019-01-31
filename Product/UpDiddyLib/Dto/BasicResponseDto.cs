using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class BasicResponseDto : BaseDto
    {
        public int StatusCode { get; set; }
        public string Description { get; set; }
    }
}
