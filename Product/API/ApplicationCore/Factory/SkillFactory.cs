using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Factory
{
    public class SkillFactory
    {

        public static Skill CreateSkill(string skillName)
        {
            Skill rVal = new Skill();
            rVal.SkillName = skillName;
            rVal.CreateDate = DateTime.UtcNow;
            rVal.ModifyDate = DateTime.UtcNow;
            rVal.CreateGuid = Guid.NewGuid();
            rVal.ModifyGuid = Guid.NewGuid();
            rVal.SkillGuid = Guid.NewGuid();
            rVal.IsDeleted = 0;
            rVal.SkillGuid = Guid.NewGuid();
            return rVal;
        }

        static public async Task<Skill> GetOrAdd(IRepositoryWrapper repositoryWrapper, string skillName)
        {
            skillName = skillName.Trim().ToLower();

            Skill skill = repositoryWrapper.SkillRepository.GetAllWithTracking()
                .Where(s => s.IsDeleted == 0 && s.SkillName == skillName)
                .FirstOrDefault();

            if (skill == null)
            {
                skill = CreateSkill(skillName);
                await repositoryWrapper.SkillRepository.Create(skill);
                await repositoryWrapper.SkillRepository.SaveAsync();
            }
            return skill;
        }


        static public async Task<Skill> GetSkillByGuid(IRepositoryWrapper repositoryWrapper, Guid skillGuid)
        {
            return await repositoryWrapper.SkillRepository.GetAllWithTracking()
               .Where(s => s.IsDeleted == 0 && s.SkillGuid == skillGuid)
               .FirstOrDefaultAsync();
        }


    }
}
