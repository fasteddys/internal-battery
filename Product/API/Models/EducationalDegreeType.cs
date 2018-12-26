using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class EducationalDegreeType : BaseModel
    {
        public int EducationalDegreeTypeId { get; set; }
        public Guid EducationalDegreeTypeGuid { get; set; }
        public string DegreeType { get; set; }

    }
}
