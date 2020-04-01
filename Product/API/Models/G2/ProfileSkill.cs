using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.G2
{
    [Table("ProfileSkills", Schema = "G2")]
    public class ProfileSkill : BaseModel
    {
        public int ProfileSkillId { get; set; }
        public Guid ProfileSkillGuid { get; set; }
        public int ProfileId { get; set; }
        public virtual Profile Profile { get; set; }
        public int SkillId { get; set; }
        public virtual Skill Skill { get; set; }
    }
}
