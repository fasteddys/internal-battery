using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class JobPostingDto : BaseDto
    {       
        public Guid? JobPostingGuid { get; set; }
        
        public DateTime PostingDateUTC { get; set; }
        
        public DateTime PostingExpirationDateUTC { get; set; }
 
        public DateTime ApplicationDeadlineUTC { get; set; }

        public string CloudTalentUri { get; set; }
        
        public int CloudTalentIndexStatus { get; set; }  
     
        public CompanyDto  Company { get; set; }
     

        public IndustryDto Industry { get; set; }
 
        public string Title { get; set; }
       
        public string Description { get; set; }
       
        public SecurityClearanceDto SecurityClearance { get; set; }
  
        public EmploymentTypeDto EmploymentType { get; set; } 

        public bool H2Visa { get; set; }
        public int TelecommutePercentage { get; set; }
        public Decimal Compensation { get; set; }
        public int? CompensationTypeId { get; set; }

        public CompensationTypeDto CompensationType { get; set; }
        public Boolean ThirdPartyApply { get; set; }
        public string ThirdPartyApplicationUrl { get; set; }
        public string Location { get; set; }
        public ExperienceLevelDto ExperienceLevel { get; set; }

        public EducationLevelDto EducationLevel { get; set; }

        public List<SkillDto> Skills { get; set; }


    }
}
