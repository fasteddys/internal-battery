using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class EducationalInstitutionDto : BaseDto
    { 
        public Guid EducationalInstitutionGuid { get; set; }
        public string Name { get; set; }        
    }
}
