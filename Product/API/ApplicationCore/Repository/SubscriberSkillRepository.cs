using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SubscriberSkillRepository : UpDiddyRepositoryBase<SubscriberSkill>, ISubscriberSkillRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public SubscriberSkillRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<SubscriberSkill> GetBySubscriberGuidAndSkillGuid(Guid subscriberGuid, Guid skillGuid)
        {
            return await (from ss in _dbContext.SubscriberSkill
                          join s in _dbContext.Subscriber on ss.SubscriberId equals s.SubscriberId
                          join sk in _dbContext.Skill on ss.SkillId equals sk.SkillId
                          where sk.SkillGuid == skillGuid && s.SubscriberGuid == subscriberGuid
                          select ss).FirstOrDefaultAsync();
        }

        public async Task<List<SubscriberSkill>> GetBySubscriberGuid(Guid SubscriberGuid)
        {
            return await (from ss in _dbContext.SubscriberSkill
                          join s in _dbContext.Subscriber on ss.SubscriberId equals s.SubscriberId
                          join sk in _dbContext.Skill on ss.SkillId equals sk.SkillId
                          where s.SubscriberGuid == SubscriberGuid && ss.IsDeleted == 0
                          select ss).Include(x => x.Skill).ToListAsync();
        }
    }
}
