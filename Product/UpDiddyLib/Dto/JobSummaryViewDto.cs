using System;
using System.Collections.Generic;
using System.Text;
using UpDiddyLib.Domain.Models;
namespace UpDiddyLib.Dto
{
    public class JobSummaryViewDto : JobBaseDto
 
    {

       
        public Guid JobPostingGuid { get; set; }
        /// <summary>
        ///  Date posting goes live
        /// </summary>
        public DateTime PostingDateUTC { get; set; }

        public DateTime ExpirationDateUTC { get; set; }
  
        public string Title { get; set; }
 
        public string City { get; set; }

        public string Province { get; set; }
        public string SemanticJobPath { get; set; }
        public bool? HasApplied { get; set; }

    }
}
