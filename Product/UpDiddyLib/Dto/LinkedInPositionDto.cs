using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class LinkedInPositionDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public string CompanyName { get; set; }
 
    }
}
