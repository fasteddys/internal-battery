using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SubscriberEducationHistoryRepository : UpDiddyRepositoryBase<SubscriberEducationHistory>, ISubscriberEducationHistoryRepository
    {
        public SubscriberEducationHistoryRepository(UpDiddyDbContext dbContext) : base(dbContext) { }
    }
}
