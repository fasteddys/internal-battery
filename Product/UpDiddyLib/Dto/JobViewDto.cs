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

        /// <summary>
        /// Experience level required for job
        /// </summary>
        public string ExperienceLevel { get; set; }
        /// <summary>
        /// Education level required for job
        /// </summary>
        public string EducationLevel { get; set; }
        /// <summary>
        /// Annual compensation for job 
        /// </summary>
        public long AnnualCompensation { get; set; }
        /// <summary>
        /// Job's employment type 
        /// </summary>
        public string EmploymentType { get; set; }
        /// <summary>
        /// Url to third party site for offsite applications 
        /// </summary>
        public string ThirdPartyApplyUrl { get; set; }
        /// <summary>
        /// The date the last time the job was uodated
        /// </summary>
        public DateTime ModifyDate { get; set; }
        /// <summary>
        /// The perecentage of time the job allows telecommuting 
        /// </summary>
        public long TelecommutePercentage { get; set; }
        /// <summary>
        /// Flag indicating if job application is on a third party site 
        /// </summary>
        public bool ThirdPartyApply { get; set; }

        /// <summary>
        /// The industry the job is associated with 
        /// </summary>
        public string Industry { get; set; }

        /// <summary>
        /// The industry the job is associated with 
        /// </summary>
        public string JobCategory { get; set; }

        public List<string> Skills = new List<string>();

        /// <summary>
        /// The location that was indexed into google 
        /// </summary>
        public string Location { get; set; }

        public string Country { get; set; }

     
        public string City { get; set; }
 

        public string Province { get; set; }
 
        public string PostalCode { get; set; }

    
        public string StreetAddress { get; set; }

        public int CommuteTime { get; set; }


    }
}
