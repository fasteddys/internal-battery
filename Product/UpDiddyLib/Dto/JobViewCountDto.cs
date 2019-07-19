using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class JobViewCountDto
    {
        public Guid JobPostingGuid { get; set; }
        public string JobName { get; set; }
        public int Count { get; set; }
    }
}
