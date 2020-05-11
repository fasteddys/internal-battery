using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class Company : BaseModel
    {
        public int CompanyId { get; set; }
        public Guid CompanyGuid { get; set; }
        public string CompanyName { get; set; }
        public int IsJobPoster { get; set; }
        public int IsHiringAgency { get; set; }
        public string LogoUrl { get; set; }
        /// <summary>
        /// The uri returned from google talent cloud for identifying the company in the
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
        public IList<RecruiterCompany> RecruiterCompanies { get; set; }
        /// <summary>
        /// Keeping this simple for now since there was no specific requirement. Discussed 
        /// with team members and decided against normalizing this, adding ranges, etc.
        /// </summary>
        public int? EmployeeSize { get; set; }
        [Url]
        public string WebsiteUrl { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
        public int? IndustryId { get; set; }
        public virtual Industry Industry { get; set; }
    }
}