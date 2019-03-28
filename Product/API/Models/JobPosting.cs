using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;


namespace UpDiddyApi.Models
{


    public enum JobPostingIndexStatus { NotIndexed = 0, Indexed, DeletedFromIndex, IndexError, IndexDeleteError, IndexUpdateError, UpdateIndexPending  };




    /*
     * 
     * 
     * Job postings in CareerCircle need to allow the following elements to asssit with navigating or filtering jobs as follows:
 
Job location - This could be a number of elements to assist:
  Country, ZIP, State, City, address, lat/long
  enabling distance to job/commute search capabilities
  enabling filtering by city,state
 
   
Category / Industry
 
Sub-Category
  
 
Skills ( 0 or more per posting)
 
 */


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
        /// Guid of the company that owns the posting
        /// </summary>

        public int CompanyId { get; set; }

        public Company Company { get; set; }
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

        public List<JobPostingSkill> JobPostingSkills { get; } = new List<JobPostingSkill>();


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
        /// free format location text e.g 7312 Parkway Drive Towson MD 21204
        /// </summary>
        public string Location { get; set; } 

    }
}
