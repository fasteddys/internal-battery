using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class EducationalDegreeTypeCategory : BaseModel
    {
        public int EducationalDegreeTypeCategoryId { get; set; }
        public Guid EducationalDegreeTypeCategoryGuid { get; set; }
        public string Name { get; set; }
        public int? Sequence { get; set; }
    }
}
