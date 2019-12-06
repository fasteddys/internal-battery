using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class ButterCMSRequestDto
    {

        public string Url { get; set; }
        public string  [] Keys  { get; set; }
        public Dictionary<string, string> QueryParameters { get; set; }
    }
}
