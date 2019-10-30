using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Factory
{
    public class SubscriberSkillFactory
    {
        public static async Task<bool> AddSkillForSubscriber(IRepositoryWrapper repositoryWrapper, Subscriber subscriber, Skill skill)
        {
            bool rVal = true;
            try
            {
                // check for a matching skill (either deleted or active )for this subscriber  
                var existingSkill = await repositoryWrapper.SubscriberSkillRepository.GetAll()
                    .Where(ss => ss.SubscriberId == subscriber.SubscriberId && ss.SkillId == skill.SkillId)
                    .FirstOrDefaultAsync();


                if (existingSkill != null)
                {
                    // if its and active existing skill just return true
                    if (existingSkill.IsDeleted == 0)
                        return true;

                    // if the skill was logically deleted, remove that flag and mark it as modified
                    existingSkill.IsDeleted = 0;
                    existingSkill.ModifyDate = DateTime.UtcNow;
                    await repositoryWrapper.SubscriberSkillRepository.Create(existingSkill);
                }
                else
                {
                    SubscriberSkill subscriberSkill = new SubscriberSkill()
                    {
                        SkillId = skill.SkillId,
                        SubscriberId = subscriber.SubscriberId,
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        ModifyDate = DateTime.UtcNow,
                        ModifyGuid = Guid.Empty,
                        IsDeleted = 0,
                        SubscriberSkillGuid = Guid.NewGuid()
                    };
                    await repositoryWrapper.SubscriberSkillRepository.Create(existingSkill);
                }
                await repositoryWrapper.SubscriberSkillRepository.SaveAsync();
            }
            catch
            {
                rVal = false;
            }
            return rVal;
        }

        public static async Task<SubscriberSkill> GetSkillForSubscriber(IRepositoryWrapper repositoryWrapper, Subscriber subscriber, Skill skill)
        {
            return await repositoryWrapper.SubscriberSkillRepository.GetAll()
                .Where(ss => ss.IsDeleted == 0 && ss.SkillId == skill.SkillId && ss.SubscriberId == subscriber.SubscriberId)
                .FirstOrDefaultAsync();
        }
    }
}
