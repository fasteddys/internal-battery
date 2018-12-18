using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public partial class Skill : BaseModel
    {
        public int SkillId { get; set; }
        [Required]
        public string SkillName { get; set; }
        public Guid? SkillGuid { get; set; }
    }
}
