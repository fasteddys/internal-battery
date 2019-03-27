using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class JobPostingSkill : BaseModel
    {    
        public int JobPostingId { get; set; }
        public virtual JobPosting JobPosting { get; set; }       
        public int SkillId { get; set; }
        public virtual Skill Skill { get; set; }
        public Guid? JobPostingSkillGuid { get; set; }
    }
}
