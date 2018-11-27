using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class ValueResponseDto
    {
        public int StatusCode { get; set; }
        public string ValueString { get; set; }
        public long ValueLong { get; set; }

        public DateTime? ValueDateTime { get; set; }
    }
}
