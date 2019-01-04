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
            this.CreateDate = DateTime.UtcNow;
            this.CreateGuid = Guid.Empty;
            this.ModifyDate = DateTime.UtcNow;
            this.ModifyGuid = Guid.Empty;
            this.IsDeleted = 0;
            this.SkillGuid = Guid.NewGuid();
        }

        static public Skill GetOrAdd(UpDiddyDbContext db, string skillName)
        {
            skillName = skillName.Trim();

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
