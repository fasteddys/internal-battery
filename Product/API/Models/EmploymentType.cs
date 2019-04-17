using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace UpDiddyApi.Models
{
    public class EmploymentType : BaseModel
    {
        public int EmploymentTypeId { get; set; }
        public Guid EmploymentTypeGuid { get; set; }
        public string Name { get; set; }
    }
}
