using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
   public class CompanyDto : BaseDto
    {
      
        public Guid CompanyGuid { get; set; }
        public string CompanyName { get; set; }
        public string JobPageBoilerplate { get; set; }
        public bool IsHiringAgency { get; set; }
        public bool IsJobPoster { get; set; }
    }
}
