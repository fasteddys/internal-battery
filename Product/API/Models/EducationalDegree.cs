using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models 
{
    public partial class EducationalDegree : BaseModel
    {
        public int EducationalDegreeId { get; set; }
        public Guid EducationalDegreeGuid { get; set; }
        public string Degree { get; set; }
    }
}
