using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public partial class EducationalInstitution : BaseModel
    {
        public int EducationalInstitutionId { get; set; }
        public Guid EducationalInstitutionGuid { get; set; }
        public string Name { get; set; }
    }
}
