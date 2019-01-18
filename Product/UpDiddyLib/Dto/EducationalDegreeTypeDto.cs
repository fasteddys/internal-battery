using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class EducationalDegreeTypeDto : BaseDto
    {
        public int EducationalDegreeTypeId { get; set; }
        public Guid EducationalDegreeTypeGuid { get; set; }
        public string DegreeType { get; set; }
    }
}