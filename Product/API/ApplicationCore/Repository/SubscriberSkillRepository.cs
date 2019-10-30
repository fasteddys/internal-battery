using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SubscriberSkillRepository : UpDiddyRepositoryBase<SubscriberSkill>, ISubscriberSkillRepository
    {
        public SubscriberSkillRepository(UpDiddyDbContext dbContext) : base(dbContext) { }
    }
}
