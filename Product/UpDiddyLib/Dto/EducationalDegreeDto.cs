using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class EducationalDegreeDto : BaseDto
    {
        public Guid EducationalDegreeGuid { get; set; }
        public string Degree { get; set; }

    }
}
