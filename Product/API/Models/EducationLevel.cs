using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class EducationLevel : BaseModel
    {
        public int EducationLevelId { get; set; }
        public Guid EducationLevelGuid { get; set; }
        [Required]
        public string Level { get; set; }

    }
}
