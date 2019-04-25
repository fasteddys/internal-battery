using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class JobPostingSkillDto
    {
   
        public virtual JobPostingDto JobPosting { get; set; }
 
        public virtual SkillDto Skill { get; set; }
        public Guid? JobPostingSkillGuid { get; set; }
    }
}
