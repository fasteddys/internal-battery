using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;


namespace UpDiddyLib.Dto
{
    public class JobViewDto
    {

        /// <summary>
        /// From Google: A summary of the job with core information that's displayed on the search results listing page.
        /// </summary>
        public string JobSummary { get; set; }
        /// <summary>
        /// From Google: Contains snippets of text from the [Job.job_title][] field most closely matching a search query's keywords, 
        /// if available. The matching query keywords are enclosed in HTML bold tags.
        /// </summary>
        public string JobTitleSnippet { get; set;  }

        /// <summary>
        /// From Google: Contains snippets of text from the Job.description and similar fields that most closely match a search query's keywords, 
        /// if available. All HTML tags in the original fields are stripped when returned in this field, and matching query keywords are enclosed in HTML bold tags.
        /// </summary>
        public string SearchTextSnippet { get; set; }

        public Guid JobPostingGuid { get; set; }
        /// <summary>
        ///  Date posting goes live
        /// </summary>
        public DateTime PostingDateUTC { get; set; }
        /// <summary>
        /// Date posting is removed from site 
        /// </summary>
        public DateTime PostingExpirationDateUTC { get; set; }

        /// <summary>
        /// The date until which applications will be accepted 
        /// </summary>
        public DateTime ApplicationDeadlineUTC { get; set; }

        /****************  google talent cloud information   ****************************/
        /// <summary>
        /// The uri returned from google talent cloud for identifying the posting in the
        /// talent cloud
        /// </summary>
        public string CloudTalentUri { get; set; }

        public string  CompanyName { get; set; }
        /// <summary>
        /// Guid of industry associated with the job 
        /// </summary>
 
        public string Title { get; set; }
        /// <summary>
        /// Job posting text 
        /// </summary>
        public string Description { get; set; }

        public string ExperienceLevel { get; set; }

        public string EducationLevel { get; set; }

        public long AnnualCompensation { get; set; }
        public string EmploymentType { get; set; }
        public string ThirdPartyApplyUrl { get; set; }
        public DateTime ModifyDate { get; set; }
        public long TelecommutePercentage { get; set; }
        public bool ThirdPartyApply { get; set; }
  
    }
}
