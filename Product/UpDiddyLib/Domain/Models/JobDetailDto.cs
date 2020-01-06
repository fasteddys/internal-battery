using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class JobDetailDto : JobBaseDto
    {


        public List<string> Skills = new List<string>();

        public string Country { get; set; }

        public string City { get; set; }


        public string Province { get; set; }

        public string PostalCode { get; set; }


        public string StreetAddress { get; set; }

        public int CommuteTime { get; set; }

        public string SemanticJobPath { get; set; }


        public Guid JobPostingGuid { get; set; }

        public DateTime PostingDateUTC { get; set; }

        public DateTime PostingExpirationDateUTC { get; set; }

        public DateTime ApplicationDeadlineUTC { get; set; }

        public string Description { get; set; }

        public string ExperienceLevel { get; set; }
 
        public string EducationLevel { get; set; }
        public long AnnualCompensation { get; set; }

        public string EmploymentType { get; set; }

        public string ThirdPartyApplicationUrl { get; set; }
        public DateTime ModifyDate { get; set; }

        public long TelecommutePercentage { get; set; }
 
        public bool ThirdPartyApply { get; set; }
 
        public string Industry { get; set; }
     
        public string JobCategory { get; set; }

        public string Title { get; set; }
 
    }
}
