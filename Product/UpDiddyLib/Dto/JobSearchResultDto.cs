using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class JobSearchResultDto
    {       
        public int PageNum { get; set; }
        public List<JobPostingDto> Jobs { get; set; } = new List<JobPostingDto>();
    }
}
