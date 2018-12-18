using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public partial class Skill
    {

        #region Factory Methods

        public Skill(string skillName)
        {
            this.SkillName = skillName;
            this.CreateDate = DateTime.Now;
            this.CreateGuid = Guid.NewGuid();
            this.ModifyDate = DateTime.Now;
            this.ModifyGuid = Guid.NewGuid();
            this.IsDeleted = 0;
        }

        static public Skill GetOrAdd(UpDiddyDbContext db, string skillName)
        {
            Skill skill = db.Skill
                .Where(s => s.IsDeleted == 0 && s.SkillName == skillName)
                .FirstOrDefault();

            if (skill == null)
            {
                skill = new Skill(skillName);
                db.Skill.Add(skill);
                db.SaveChanges();
            }
            return skill;
        }

        #endregion

    }
}
