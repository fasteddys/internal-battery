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
        public int? Sequence { get; set; }
        public bool? IsVerified { get; set; }

        public int? EducationalDegreeTypeCategoryId { get; set; }
        public virtual EducationalDegreeTypeCategory EducationalDegreeTypeCategory { get; set; }
    }
}
