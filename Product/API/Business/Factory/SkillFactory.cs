using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.Business.Factory
{
    public class SkillFactory
    {

        public static  Skill CreateSkill(string skillName)
        {
            Skill rVal = new Skill();
            rVal.SkillName = skillName;
            rVal.CreateDate = DateTime.Now;
            rVal.CreateGuid = Guid.NewGuid();
            rVal.ModifyDate = DateTime.Now;
            rVal.ModifyGuid = Guid.NewGuid();
            rVal.SkillGuid = Guid.NewGuid();
            rVal.IsDeleted = 0;
            return rVal;
        }

        static public Skill GetOrAdd(UpDiddyDbContext db, string skillName)
        {
            skillName = skillName.Trim();

            Skill skill = db.Skill
                .Where(s => s.IsDeleted == 0 && s.SkillName == skillName)
                .FirstOrDefault();

            if (skill == null)
            {
                skill =  CreateSkill(skillName);
                db.Skill.Add(skill);
                db.SaveChanges();
            }
            return skill;
        }
    }
}
