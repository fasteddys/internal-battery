using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;


namespace UpDiddyApi.Models
{


    public enum JobPostingIndexStatus { NotIndexed = 0, Indexed, DeletedFromIndex, IndexError, IndexDeleteError, IndexUpdateError, UpdateIndexPending  };

 
    public class JobPosting : BaseModel
    {
        public int JobPostingId { get; set; }
        /// <summary>
        /// Posting Guid
        /// </summary>
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
        /// <summary>
        ///  The postings google cloud indexing status, see enum JobPostingIndexStatus
        /// </summary>
        public int CloudTalentIndexStatus { get; set; }
        /// <summary>
        /// Additional information such as error received from cloud talent
        /// </summary>
        public string CloudTalentIndexInfo { get; set; }

        /**************** job posting data   ****************************/
        /// <summary>
        /// Private postings - never indexed into google 
        /// </summary>
        public int IsPrivate { get; set; }
        /// <summary>
        /// Status of the job e.g. Draft, Live, Expired etc.   
        /// </summary>
        public int JobStatus { get; set; }

        /// <summary>
        /// The recruiter (which may or may not also be a subscriber) who posted the job 
        /// </summary>
        public int? RecruiterId { get; set; }

        public virtual Recruiter Recruiter { get; set; }

        /// <summary>
        /// Guid of the company that owns the posting
        /// </summary>
        public int? CompanyId { get; set; }

        public virtual Company Company { get; set; }
        /// <summary>
        /// Guid of industry associated with the job 
        /// </summary>
        public int? IndustryId { get; set; }

        public virtual Industry Industry { get; set; }

        /// <summary>
        /// Subcategorizaion of the job
        /// </summary>
        public int? JobCategoryId { get; set; }

        public virtual JobCategory JobCategory  { get; set;}


        /// <summary>
        /// Job posting's title 
        /// </summary>
        ///         

        public string Title { get; set; }
        /// <summary>
        /// Job posting text 
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Security level required for position 
        /// </summary>
        public int? SecurityClearanceId { get; set; }
        public virtual SecurityClearance SecurityClearance { get; set; }
        /// <summary>
        /// Position employment type  e.g. direct hire, contractor, part time, etc 
        /// </summary>
        
        public int? EmploymentTypeId { get; set; }

        public virtual EmploymentType EmploymentType { get; set; }

        /// <summary>
        ///  Flag indicating if the job poster is a staffing agency
        /// </summary>
        public bool IsAgencyJobPosting { get; set; }


        /// <summary>
        /// Flag indicating if company offer H2 Visas
        /// </summary>
        public bool H2Visa { get; set; }
        /// <summary>
        /// Percentage of time working telecommute is allowd 
        /// </summary>
        public int TelecommutePercentage { get; set; }
        /// <summary>
        /// Dollar compensation amount 
        /// </summary>
        public Decimal Compensation { get; set; }
        /// <summary>
        /// Compensatopn type indicator e.g. hourly, weekly, annual, etc
        /// </summary>
        public int? CompensationTypeId { get; set; }

        public virtual CompensationType CompensationType { get; set; }

        public int? ExperienceLevelId { get; set; }
       
        public virtual ExperienceLevel ExperienceLevel { get; set; }

        public int? EducationLevelId { get; set; }

        public virtual EducationLevel EducationLevel { get; set; }

        public List<JobPostingSkill> JobPostingSkills { get; set; } = new List<JobPostingSkill>();


        /****************  application information  ****************************/
        /// <summary>
        /// Flag indicating if job application is on CC or a parter site
        /// </summary>
        public Boolean ThirdPartyApply { get; set; }
        /// <summary>
        /// Url to apply if applications are on parter site 
        /// </summary>
        public string ThirdPartyApplicationUrl { get; set; }

        /****************  job posting geographical information  ****************************/

        /// <summary>
        /// Country of job posting 
        /// </summary>
        public string Country { get; set; }
  
        /// <summary>
        /// name of the city of job posting 
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// state or provience of job posting 
        /// </summary>

        public string Province  { get; set; }

        /// <summary>
        /// postal code of job posting 
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// street address of posting 
        /// </summary>
        public string StreetAddress { get; set; }

        /// <summary>
        /// Jobs pulled from third party sites will have a value here
        /// </summary>
        public string ThirdPartyIdentifier { get; set; }
    }
}
