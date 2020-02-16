using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UpDiddyLib.Dto;

namespace UpDiddyLib.Domain.Models
{
    public class JobCrudDto
    {
        [JsonIgnore]
        public int TotalRecords { get; set;  }
        public Guid? JobPostingGuid { get; set; }
        public DateTime PostingDateUTC { get; set; }
        public DateTime PostingExpirationDateUTC { get; set; }
        public DateTime ApplicationDeadlineUTC { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool H2Visa { get; set; }
        public bool IsAgencyJobPosting { get; set; }
        public int TelecommutePercentage { get; set; }
        public Decimal Compensation { get; set; }
        public Boolean ThirdPartyApply { get; set; }
        public string ThirdPartyApplicationUrl { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public string StreetAddress { get; set; }
        public int IsPrivate { get; set; }
        public int JobStatus { get; set; }
        public string ThirdPartyIdentifier { get; set; }
 
        // Guids for related object 
        public Guid RecruiterGuid { get; set; }
        public Guid CompanyGuid { get; set; }
        public Guid IndustryGuid { get; set; }
        public Guid JobCategoryGuid { get; set; }
        public Guid ExperienceLevelGuid { get; set; }
        public Guid EducationLevelGuid { get; set; }
        public Guid CompensationTypeGuid { get; set; }
        public Guid SecurityClearanceGuid { get; set; }
        public Guid EmploymentTypeGuid { get; set; }
    }
}