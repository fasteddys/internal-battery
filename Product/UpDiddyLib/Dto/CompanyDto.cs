using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
   public class CompanyDto : BaseDto
    {
      
        public Guid CompanyGuid { get; set; }
        public string CompanyName { get; set; }
    }
}
