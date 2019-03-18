using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class JobPostingDto
    {
        
        public int JobPostingId { get; set; }
        
        public Guid? JobPostingGuid { get; set; }
        
        public DateTime PostingDateUTC { get; set; }
        
        public DateTime PostingExpirationDateUTC { get; set; }
        
        public string GoogleCloudUri { get; set; }
        
        public int GoogleCloudIndexStatus { get; set; }  
     
        public CompanyDto  Company { get; set; }
     

        public IndustryDto Industry { get; set; }
 
        public string Title { get; set; }
       
        public string Description { get; set; }
       

        SecurityClearanceDto SecurityClearance { get; set; }
 
 
        public List<EmploymentTypeDto> EmploymentTypes { get; set; } = new List<EmploymentTypeDto>();
        public bool H2Visa { get; set; }
        public int TelecommutePercentage { get; set; }
        public Decimal Compensation { get; set; }
        public int? CompensationTypeId { get; set; }

        CompensationTypeDto CompensationType { get; set; }
        public Boolean ThirdPartyApply { get; set; }
        public string ThirdPartyApplicationUrl { get; set; }
        public string Location { get; set; }


        


    }
}
