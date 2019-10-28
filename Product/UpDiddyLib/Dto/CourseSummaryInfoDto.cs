using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class CourseSummaryInfoDto 
    {
        public string Title { get; set; }
        public string Details { get; set; }

        public string Vendor { get; set; }

        public bool IsNew { get; set; }

        public int NumEnrollments { get; set; }


    }
}
