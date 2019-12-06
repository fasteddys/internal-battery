using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class JobSummaryViewDto
 
    {

       
        public Guid JobPostingGuid { get; set; }
        /// <summary>
        ///  Date posting goes live
        /// </summary>
        public DateTime PostingDateUTC { get; set; }

        public DateTime ExpirationDateUTC { get; set; }

        public string CompanyLogoUrl { get; set; }
        public string CompanyName { get; set; }
  
        public string Title { get; set; }
 
        public string City { get; set; }

        public string Province { get; set; }
        public string SemanticJobPath { get; set; }
        public bool? HasApplied { get; set; }

    }
}
