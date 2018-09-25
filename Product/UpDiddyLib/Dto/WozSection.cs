using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class WozSectionDto
    {
        public string courseCode { get; set; }
        public long startDateUTC { get; set; }
        public long endDateUTC { get; set; }
        public bool isOpen { get; set; }
        public int maxStudents { get; set; }
        public string timeZone { get; set; } 
    }
}
