using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.G2
{
    [Table("ProfileEmploymentTypes", Schema = "G2")]
    public class ProfileEmploymentType : BaseModel
    {
        public int ProfileEmploymentTypeId { get; set; }
        public Guid ProfileEmploymentTypeGuid { get; set; }
        public int ProfileId { get; set; }
        public virtual Profile Profile { get; set; }
        public int EmploymentTypeId { get; set; }
        public virtual EmploymentType EmploymentType { get; set; }
    }
}
