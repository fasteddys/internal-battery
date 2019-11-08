using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class CampaignRepository : UpDiddyRepositoryBase<Campaign>, ICampaignRepository
    {
        public CampaignRepository(UpDiddyDbContext dbContext) : base(dbContext) { }
    }
}
