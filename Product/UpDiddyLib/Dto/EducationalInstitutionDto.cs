using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class EducationalInstitutionDto : BaseDto
    {
        public int EducationalInstitutionId { get; set; }
        public Guid EducationalInstitutionGuid { get; set; }
        public string Name { get; set; }
    }
}