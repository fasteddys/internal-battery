using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class JobCrudListDto
    {
        public List<JobCrudDto> Entities { get; set; }
        public int TotalRecords { get; set; }
    }
}
