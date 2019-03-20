using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;


namespace UpDiddyApi.Models
{


    public enum JobPostingIndexStatus { NotIndexed = 0, Indexed, DeletedFromIndex, IndexError, IndexDeleteError, IndexUpdateError  };

    public class JobPosting : BaseModel
    {        
        public int JobPostingId { get; set; }
        /// <summary>
        /// Posting Guid
        /// </summary>
        public Guid? JobPostingGuid { get; set; }
        /// <summary>
        ///  Date posting goes live
        /// </summary>
        public DateTime PostingDateUTC { get; set; }
        /// <summary>
        /// Date posting is removed from site 
        /// </summary>
        public DateTime PostingExpirationDateUTC { get; set; }

        /****************  google talent cloud information   ****************************/
        /// <summary>
        /// The uri returned from google talent cloud for identifying the posting in the
        /// talent cloud
        /// </summary>
        public string GoogleCloudUri { get; set; }
        /// <summary>
        ///  The postings google cloud indexing status
        /// </summary>
        public int GoogleCloudIndexStatus { get; set; } // status of posting in google cloud job index 

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

        public Industry Industry { get; set; }
        /// <summary>
        /// Job posting's title 
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Job posting text 
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Security level required for position 
        /// </summary>
        public int? SecurityClearanceId { get; set; }
        SecurityClearance SecurityClearance { get; set; }
        /// <summary>
        /// Position employment type  e.g. direct hire, contractor, part time, etc 
        /// </summary>
        
        public int? EmploymentTypeId { get; set; }

        public EmploymentType EmploymentType { get; set; }
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

        CompensationType CompensationType { get; set; }

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
