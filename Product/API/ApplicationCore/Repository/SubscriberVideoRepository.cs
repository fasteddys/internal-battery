using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SubscriberVideoRepository : UpDiddyRepositoryBase<SubscriberVideo>, ISubscriberVideoRepository
    {
        public SubscriberVideoRepository(UpDiddyDbContext dbContext)
            : base(dbContext)
        { }
    }
}
