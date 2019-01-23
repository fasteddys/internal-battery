using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddyApi.Models
{
    public class CourseSkill : BaseDto
    {
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }
        public int SkillId { get; set; }
        public virtual Skill Skill { get; set; }
        public Guid? CourseSkillGuid { get; set; }
    }
}
